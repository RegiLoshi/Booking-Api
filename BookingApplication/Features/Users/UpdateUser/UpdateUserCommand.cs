using MediatR;

namespace BookingApplication.Features.Users.UpdateUser;

public class UpdateUserCommand : IRequest<Guid>
{
    public UpdateUserDto UpdateUserDto { get; set; } = null!;
}