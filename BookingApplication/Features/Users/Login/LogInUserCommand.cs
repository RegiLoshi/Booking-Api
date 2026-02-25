using MediatR;

namespace BookingApplication.Features.Users.Login
{
    public class LogInUserCommand : IRequest<LogInUserResponse>
    {
        public LogInUserDto LogInUserDto { get; set; } = null!;
    }
}