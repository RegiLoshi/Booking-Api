using System.Security.Claims;
using BookingApplication.Features.Booking.CreateBooking;
using MediatR;

namespace BookingApp.Endpoints;

public static class BookingEndpoints
{
    public static void MapBookingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("v1/booking").RequireAuthorization();

        group.MapPost("/", async (CreateBookingDto dto, HttpContext httpContext, IMediator mediator) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var guestId))
                return Results.Unauthorized();

            var command = new CreateBookingCommand
            {
                GuestId = guestId,
                CreateBookingDto = dto
            };

            var id = await mediator.Send(command);
            return Results.Ok(new { id });
        });
    }
}

