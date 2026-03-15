using BookingDomain.Entities;
using MediatR;

namespace BookingApplication.Features.Booking.UpdateBookingStatus;

public class UpdateBookingStatusCommand : IRequest<bool>
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public BookingStatus NewStatus { get; set; }
}

