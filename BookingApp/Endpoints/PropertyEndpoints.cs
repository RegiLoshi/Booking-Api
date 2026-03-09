using System.Security.Claims;
using BookingApplication.Features.Properties.CreateProperty;
using BookingApplication.Features.Properties.UpdateProperty;
using FluentValidation;
using MediatR;

namespace BookingApp.Endpoints;

public static class PropertyEndpoints
{
    public static void MapPropertyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("v1/property").RequireAuthorization();

        group.MapPost("/createProperty", async (CreatePropertyDto dto, HttpContext httpContext, IMediator mediator) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var ownerId))
                return Results.Unauthorized();

            var command = new CreatePropertyCommand
            {
                OwnerId = ownerId,
                CreatePropertyDto = dto
            };

            try
            {
                var result = await mediator.Send(command);
                return Results.Ok(new { id = result });
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(ex.Errors);
            }
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdatePropertyDto dto, HttpContext httpContext, IMediator mediator) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var ownerId))
                return Results.Unauthorized();

            var command = new UpdatePropertyCommand
            {
                PropertyId = id,
                OwnerId = ownerId,
                UpdatePropertyDto = dto
            };

            var updated = await mediator.Send(command);
            return updated ? Results.Ok() : Results.NotFound();
        });
    }
}