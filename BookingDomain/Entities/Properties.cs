namespace BookingDomain.Entities;


using System.ComponentModel.DataAnnotations;

public class Properties
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid OwnerId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string PropertyType { get; set; }
    public Guid AddressId { get; set; }
    public int MaxGuests { get; set; }
    public TimeSpan CheckInTime { get; set; }
    public TimeSpan CheckOutTime { get; set; }
    public bool IsActive { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public DateTime? LastBookedOnUtc { get; set; }

    public Users Owner { get; set; } = null!;
    public Address Address { get; set; } = null!;
    public ICollection<Bookings> Bookings { get; set; } = new List<Bookings>();
}
