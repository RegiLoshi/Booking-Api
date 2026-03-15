using System.Security.Claims;
using BookingApplication.Features.Booking.CreateBooking;
using BookingApplication.Features.Booking.UpdateBookingStatus;
using BookingDomain.Entities;
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

        group.MapPost("/{id:guid}/confirm", async (Guid id, HttpContext httpContext, IMediator mediator) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var hostId))
                return Results.Unauthorized();

            var command = new UpdateBookingStatusCommand
            {
                BookingId = id,
                UserId = hostId,
                NewStatus = BookingStatus.Confirmed
            };

            var success = await mediator.Send(command);
            return success ? Results.Ok() : Results.Forbid();
        });

        group.MapPost("/{id:guid}/reject", async (Guid id, HttpContext httpContext, IMediator mediator) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var hostId))
                return Results.Unauthorized();

            var command = new UpdateBookingStatusCommand
            {
                BookingId = id,
                UserId = hostId,
                NewStatus = BookingStatus.Rejected
            };

            var success = await mediator.Send(command);
            return success ? Results.Ok() : Results.Forbid();
        });

        group.MapPost("/{id:guid}/cancel", async (Guid id, HttpContext httpContext, IMediator mediator) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var guestId))
                return Results.Unauthorized();

            var command = new UpdateBookingStatusCommand
            {
                BookingId = id,
                UserId = guestId,
                NewStatus = BookingStatus.Cancelled
            };

            var success = await mediator.Send(command);
            return success ? Results.Ok() : Results.Forbid();
        });
    }
}

