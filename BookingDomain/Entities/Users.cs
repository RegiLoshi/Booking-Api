namespace BookingDomain;

using System.ComponentModel.DataAnnotations;

public class Users
{
    [Key]
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; } //hashed
    public string PhoneNumber { get; set; }
    public string ProfilePictureUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public ICollection<UserRoles> UserRoles { get; set; } = new List<UserRoles>();
    public ICollection<Properties> OwnedProperties { get; set; } = new List<Properties>();
    public OwnerProfiles? OwnerProfile { get; set; }
    public ICollection<Bookings> Bookings { get; set; } = new List<Bookings>();
    public ICollection<Reviews> Reviews { get; set; } = new List<Reviews>();
}