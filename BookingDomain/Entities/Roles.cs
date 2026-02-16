namespace BookingDomain;

using System.ComponentModel.DataAnnotations;

public class Roles
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    public ICollection<UserRoles> UserRoles { get; set; } = new List<UserRoles>();
}
