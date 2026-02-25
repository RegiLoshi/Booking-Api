namespace BookingDomain.Entities;


using System.ComponentModel.DataAnnotations;

public class Reviews{
    [Key]
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid GuestId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }
    public DateTime CreatedAt { get; set; }  

    public Bookings Booking { get; set; } = null!;
    public Users Guest { get; set; } = null!;

}