namespace BookingDomain;

public class UserRoles
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    
    public Users User { get; set; } = null!;
    public Roles Role { get; set; } = null!;
    
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
