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

        // dont allow changes from states other than Pending
        if (booking.BookingStatus != BookingStatus.Pending)
        {
            throw new InvalidOperationException("Cannot update a booking from a non-pending state.");
        }

        switch (request.NewStatus)
        {
            case BookingStatus.Confirmed:
            case BookingStatus.Rejected:
            {
                // Only the host can confirm/reject
                var property = await propertyRepository.GetById(booking.PropertyId, cancellationToken);
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
                // Only the guest can cancel their booking (before stay begins)
                if (booking.GuestId != request.UserId)
                {
                    return false;
                }

                // Allow cancel from Pending or Confirmed only
                if (booking.BookingStatus is not (BookingStatus.Pending or BookingStatus.Confirmed))
                {
                    return false;
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

