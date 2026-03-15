using MediatR;

namespace BookingApplication.Features.Booking.CreateBooking;

public class CreateBookingCommand : IRequest<Guid>
{
    public Guid GuestId { get; set; }
    public CreateBookingDto CreateBookingDto { get; set; } = null!;
}

