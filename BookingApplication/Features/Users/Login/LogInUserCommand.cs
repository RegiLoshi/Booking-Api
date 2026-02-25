using MediatR;

namespace BookingApplication.Features.Users.Login
{
    public class LogInUserCommand : IRequest<Guid>
    {
        public LogInUserDto LogInUserDto { get; set; } = null!;
    }
}