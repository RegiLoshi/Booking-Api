namespace BookingApplication.Features.Reviews.CreateReview;

public class CreateReviewDto
{
    public Guid BookingId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

