using MediatR;
using BookingApplication.Abstractions.Contracts.Repositories;
using BookingApplication.Abstractions.Contracts.AuthService;
using BCrypt.Net;

namespace BookingApplication.Features.Users.Login
{
    public class LogInUserCommandHandler : IRequestHandler<LogInUserCommand, LogInUserResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthManager _authManager;

        public LogInUserCommandHandler(IUserRepository userRepository, IAuthManager authManager)
        {
            _userRepository = userRepository;
            _authManager = authManager;
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
            return new LogInUserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Token = _authManager.GenerateToken(user)
            };
        }
    }
}