namespace BookingApplication.Features.Users.ChangePassword;
using MediatR;
public class UserChangePasswordCommand : IRequest<Guid>
{
    public UserChangePasswordDto userChangePasswordDto { get; set; }
}