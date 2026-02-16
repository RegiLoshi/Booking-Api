namespace BookingDomain;

using System.ComponentModel.DataAnnotations;

public class Bookings
{
    [Key]
    public int Id { get; set; }
    
    public int PropertyId { get; set; }
    public int GuestId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int GuestCount { get; set; }
    public decimal CleaningFee { get; set; }
    public decimal AmenitiesUpCharge { get; set; }
    public decimal PriceForPeriod { get; set; }
    public decimal TotalPrice { get; set; }
    public string BookingStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ConfirmedOnUtc { get; set; }
    public DateTime? RejectedOnUtc { get; set; }
    public DateTime? CompletedOnUtc { get; set; }
    public DateTime? CancelledOnUtc { get; set; }
    
    public Properties Property { get; set; } = null!;
    public Users Guest { get; set; } = null!;
    public Reviews? Review { get; set; }
}
