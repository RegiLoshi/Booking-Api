using MediatR;

namespace BookingApplication.Features.Reviews.CreateReview;

public class CreateReviewCommand : IRequest<Guid>
{
    public Guid GuestId { get; set; }
    public CreateReviewDto CreateReviewDto { get; set; } = null!;
}

