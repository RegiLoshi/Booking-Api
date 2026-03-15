using MediatR;
using BookingApplication.Abstractions.Contracts.Repositories;
using BookingDomain.Entities;

namespace BookingApplication.Features.Booking.MarkBookingCompleted;

public class MarkBookingCompletedCommandHandler(IBookingRepository bookingRepository)
    : IRequestHandler<MarkBookingCompletedCommand, bool>
{
    public async Task<bool> Handle(MarkBookingCompletedCommand request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetById(request.BookingId, cancellationToken);
        if (booking == null)
            return false;

        booking.BookingStatus = BookingStatus.Completed;
        booking.CompletedOnUtc = DateTime.UtcNow;
        booking.LastModifiedAt = DateTime.UtcNow;

        await bookingRepository.Update(booking, cancellationToken);
        return true;
    }
}
