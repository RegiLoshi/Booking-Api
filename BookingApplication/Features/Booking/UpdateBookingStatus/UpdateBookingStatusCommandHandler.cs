using MediatR;
using BookingApplication.Abstractions.Contracts.Repositories;
using BookingDomain.Entities;

namespace BookingApplication.Features.Booking.UpdateBookingStatus;

public class UpdateBookingStatusCommandHandler(
    IBookingRepository bookingRepository,
    IPropertyRepository propertyRepository)
    : IRequestHandler<UpdateBookingStatusCommand, bool>
{
    public async Task<bool> Handle(UpdateBookingStatusCommand request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetById(request.BookingId, cancellationToken);
        if (booking == null)
        {
            return false;
        }

        var now = DateTime.UtcNow;
        var property = await propertyRepository.GetById(booking.PropertyId, cancellationToken);
        if (property == null)
        {
            return false;
        }

        switch (request.NewStatus)
        {
            case BookingStatus.Confirmed:
            case BookingStatus.Rejected:
            {
                // Only allow confirm/reject from Pending
                if (booking.BookingStatus != BookingStatus.Pending)
                {
                    throw new InvalidOperationException("Cannot confirm/reject a booking from a non-pending state.");
                }

                // Only the host can confirm/reject
                if (property == null || property.OwnerId != request.UserId)
                {
                    return false;
                }

                // Only allow from Pending
                if (booking.BookingStatus != BookingStatus.Pending)
                {
                    return false;
                }

                if (request.NewStatus == BookingStatus.Confirmed)
                {
                    booking.BookingStatus = BookingStatus.Confirmed;
                    booking.ConfirmedOnUtc = now;
                }
                else
                {
                    booking.BookingStatus = BookingStatus.Rejected;
                    booking.RejectedOnUtc = now;
                }

                break;
            }

            case BookingStatus.Cancelled:
            {
                // Guest OR property owner can cancel (before stay begins)
                var isGuest = booking.GuestId == request.UserId;
                var isOwner = property.OwnerId == request.UserId;
                if (!isGuest && !isOwner)
                {
                    return false;
                }

                // Allow cancel from Pending or Confirmed only
                if (booking.BookingStatus is not (BookingStatus.Pending or BookingStatus.Confirmed))
                {
                    throw new InvalidOperationException("Cannot cancel a booking from its current state.");
                }

                if (now.Date >= booking.StartDate.Date)
                {
                    throw new InvalidOperationException("Cannot cancel a booking on or after the start date.");
                }

                booking.BookingStatus = BookingStatus.Cancelled;
                booking.CancelledOnUtc = now;
                break;
            }

            default:
                // Completed / Expired are set by the system; no public endpoint
                return false;
        }

        booking.LastModifiedAt = now;
        await bookingRepository.Update(booking, cancellationToken);

        return true;
    }
}

