using MediatR;

namespace BookingApplication.Features.Booking.MarkBookingCompleted;

public class MarkBookingCompletedCommand : IRequest<bool>
{
    public Guid BookingId { get; set; }
}
