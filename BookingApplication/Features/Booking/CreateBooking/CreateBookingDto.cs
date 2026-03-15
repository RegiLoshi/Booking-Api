using BookingDomain.Entities;

namespace BookingApplication.Features.Booking.CreateBooking;

public class CreateBookingDto
{
    public Guid PropertyId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int GuestCount { get; set; }
    public BookingStatus BookingStatus { get; set; }
}