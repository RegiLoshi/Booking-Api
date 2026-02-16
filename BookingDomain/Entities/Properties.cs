namespace BookingDomain;

using System.ComponentModel.DataAnnotations;

public class Properties
{
    [Key]
    public int Id { get; set; }
    
    public int OwnerId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string PropertyType { get; set; }
    public int AddressId { get; set; }
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
