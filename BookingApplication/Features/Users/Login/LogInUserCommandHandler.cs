using MediatR;
using BookingApplication.Abstractions.Contracts.Repositories;
using BookingApplication.Abstractions.Contracts.AuthService;
using BookingApplication.Abstractions.Contracts.Logging;
using LoginLogContracts;
using Microsoft.Extensions.Logging;

namespace BookingApplication.Features.Users.Login
{
    public class LogInUserCommandHandler : IRequestHandler<LogInUserCommand, LogInUserResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthManager _authManager;
        private readonly ILoginEventPublisher _loginEventPublisher;
        private readonly ILogger<LogInUserCommandHandler> _logger;

        public LogInUserCommandHandler(
            IUserRepository userRepository,
            IAuthManager authManager,
            ILoginEventPublisher loginEventPublisher,
            ILogger<LogInUserCommandHandler> logger)
        {
            _userRepository = userRepository;
            _authManager = authManager;
            _loginEventPublisher = loginEventPublisher;
            _logger = logger;
        }

        public async Task<LogInUserResponse> Handle(LogInUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByEmail(request.LogInUserDto.Email, cancellationToken);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            if (!BCrypt.Net.BCrypt.EnhancedVerify(request.LogInUserDto.Password, user.Password))
            {
                throw new Exception("Invalid password");
            }

            try
            {
                await _loginEventPublisher.PublishAsync(new LoginSucceededLogMessage
                {
                    EventId = Guid.NewGuid(),
                    OccurredAtUtc = DateTime.UtcNow,
                    UserId = user.Id,
                    Email = user.Email,
                    SourceApp = "booking-backend",
                    EventType = "user.login.succeeded"
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish login event for user {UserId}", user.Id);
            }

            return new LogInUserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Token = _authManager.GenerateToken(user)
            };
        }
    }
}
