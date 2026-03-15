using MediatR;
using BookingApplication.Abstractions.Contracts.Repositories;
using BookingDomain.Entities;

namespace BookingApplication.Features.Reviews.CreateReview;

public class CreateReviewCommandHandler(
    IReviewRepository reviewRepository,
    IBookingRepository bookingRepository)
    : IRequestHandler<CreateReviewCommand, Guid>
{
    public async Task<Guid> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var dto = request.CreateReviewDto;

        if (dto.Rating < 1 || dto.Rating > 5)
        {
            throw new ArgumentException("Rating must be between 1 and 5.");
        }

        var booking = await bookingRepository.GetById(dto.BookingId, cancellationToken);
        if (booking == null)
        {
            throw new ArgumentException("Booking not found.");
        }

        // Only the guest who made the booking can review
        if (booking.GuestId != request.GuestId)
        {
            throw new InvalidOperationException("You can only review your own bookings.");
        }

        // Only completed stays can be reviewed
        if (booking.BookingStatus != BookingStatus.Completed)
        {
            throw new InvalidOperationException("Only completed bookings can be reviewed.");
        }

        // Prevent duplicate reviews
        if (booking.Review != null)
        {
            throw new InvalidOperationException("This booking has already been reviewed.");
        }

        var now = DateTime.UtcNow;

        var review = new Reviews
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            GuestId = request.GuestId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = now
        };

        await reviewRepository.Add(review, cancellationToken);
        return review.Id;
    }
}

