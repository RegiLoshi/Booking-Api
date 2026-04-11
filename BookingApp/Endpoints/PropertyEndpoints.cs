using System.Security.Claims;
using BookingApplication.Features.Properties.CreateProperty;
using BookingApplication.Features.Properties.GetPropertyDetails;
using BookingApplication.Features.Properties.SearchProperties;
using BookingApplication.Features.Properties.UpdateProperty;
using FluentValidation;
using MediatR;

namespace BookingApp.Endpoints;

public static class PropertyEndpoints
{
    public static void MapPropertyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("v1/property");
        var authGroup = group.MapGroup(string.Empty).RequireAuthorization();

        group.MapGet("/search", async (
            string? country,
            string? city,
            string? zipCode,
            string? propertyType,
            DateTime? startDate,
            DateTime? endDate,
            int? guests,
            decimal? minPricePerDay,
            decimal? maxPricePerDay,
            double? minRating,
            PropertySearchSort? sort,
            int? page,
            int? pageSize,
            int[]? amenities,
            IMediator mediator) =>
        {
            var req = new SearchPropertiesRequest
            {
                Country = country,
                City = city,
                ZipCode = zipCode,
                PropertyType = propertyType,
                StartDate = startDate,
                EndDate = endDate,
                Guests = guests,
                MinPricePerDay = minPricePerDay,
                MaxPricePerDay = maxPricePerDay,
                MinRating = minRating,
                Sort = sort ?? PropertySearchSort.PriceAsc,
                Page = page ?? 1,
                PageSize = pageSize ?? 20,
                Amenities = amenities?.ToList()
            };

            try
            {
                var result = await mediator.Send(new SearchPropertiesQuery { Request = req });
                return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetPropertyDetailsQuery { PropertyId = id });
            return result == null ? Results.NotFound() : Results.Ok(result);
        });

        authGroup.MapPost("/createProperty", async (CreatePropertyDto dto, HttpContext httpContext, IMediator mediator) =>
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

        authGroup.MapPut("/{id:guid}", async (Guid id, UpdatePropertyDto dto, HttpContext httpContext, IMediator mediator) =>
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
