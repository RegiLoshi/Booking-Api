using MediatR;

namespace BookingApplication.Features.Users.Register;

// mund te kthejme vetem id ose cdo property

public class LogInUserCommand : IRequest<Guid>
{
    public LogInUserDto LogInUserDto { get; set; } = null!;
}