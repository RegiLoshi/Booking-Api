namespace BookingDomain;

using System.ComponentModel.DataAnnotations;

public class Reviews{
    [Key]
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int GuestId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }
    public DateTime CreatedAt { get; set; }  

    public Bookings Booking { get; set; } = null!;
    public Users Guest { get; set; } = null!;

}