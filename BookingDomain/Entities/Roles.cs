namespace BookingDomain.Entities;


using System.ComponentModel.DataAnnotations;

public class Roles
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    public ICollection<UserRoles> UserRoles { get; set; } = new List<UserRoles>();
}
