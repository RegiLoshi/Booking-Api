using System.Security.Claims;
using BookingApplication.Features.Reviews.CreateReview;
using BookingApplication.Abstractions.Contracts.Repositories;
using MediatR;

namespace BookingApp.Endpoints;

public static class ReviewEndpoints
{
    public static void MapReviewEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("v1/review").RequireAuthorization();

        group.MapPost("/", async (CreateReviewDto dto, HttpContext httpContext, IMediator mediator) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var guestId))
                return Results.Unauthorized();

            var command = new CreateReviewCommand
            {
                GuestId = guestId,
                CreateReviewDto = dto
            };

            var id = await mediator.Send(command);
            return Results.Ok(new { id });
        });

        group.MapGet("/property/{propertyId:guid}/average", async (Guid propertyId, IReviewRepository reviewRepository, CancellationToken cancellationToken) =>
        {
            var average = await reviewRepository.GetAverageRatingForProperty(propertyId, cancellationToken);
            return Results.Ok(new { propertyId, averageRating = average });
        });
    }
}

