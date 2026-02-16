using MediatR;

namespace BookingApplication.Features.Users.Register;

// mund te kthejme vetem id ose cdo property

public class RegisterUserCommand : IRequest<Guid>
{
    public CreateUserDto CreateUserDto { get; set; } = null!;
}