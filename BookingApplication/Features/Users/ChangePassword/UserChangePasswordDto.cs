namespace BookingApplication.Features.Users.ChangePassword;

public class UserChangePasswordDto
{
    public Guid Id { get; set; }
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword  { get; set; } = string.Empty;
}