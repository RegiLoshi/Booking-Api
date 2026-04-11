using System.Security.Claims;
using BookingApplication.Abstractions.Contracts.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookingApp.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("v1/admin").RequireAuthorization();

        group.MapGet("/users", async (HttpContext httpContext, IUserRepository userRepository, CancellationToken cancellationToken) =>
        {
            if (!IsAdmin(httpContext.User))
                return Results.Forbid();

            var users = await userRepository.GetAllUsers(cancellationToken);
            var payload = users.Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.PhoneNumber,
                u.ProfilePictureUrl,
                u.IsActive,
                u.CreatedAt,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
            });

            return Results.Ok(payload);
        });

        group.MapPost("/users/{id:guid}/suspend", async (Guid id, HttpContext httpContext, IUserRepository userRepository, CancellationToken cancellationToken) =>
        {
            if (!IsAdmin(httpContext.User))
                return Results.Forbid();

            var updated = await userRepository.SetUserActiveStatus(id, false, cancellationToken);
            return updated ? Results.Ok() : Results.NotFound();
        });

        group.MapDelete("/users/{id:guid}", async (Guid id, HttpContext httpContext, IUserRepository userRepository, CancellationToken cancellationToken) =>
        {
            if (!IsAdmin(httpContext.User))
                return Results.Forbid();

            var existing = await userRepository.GetUserById(id, cancellationToken);
            if (existing == null)
                return Results.NotFound();

            await userRepository.DeleteUser(id, cancellationToken);
            return Results.Ok();
        });

        group.MapGet("/properties", async (HttpContext httpContext, IPropertyRepository propertyRepository, CancellationToken cancellationToken) =>
        {
            if (!IsAdmin(httpContext.User))
                return Results.Forbid();

            var properties = await propertyRepository.GetAllProperties(cancellationToken);
            var payload = properties.Select(p => new
            {
                p.Id,
                p.OwnerId,
                p.Name,
                p.PropertyType,
                p.PricePerDay,
                p.IsActive,
                p.IsApproved,
                p.CreatedAt,
                Address = new
                {
                    p.Address.Country,
                    p.Address.City,
                    p.Address.Street,
                    p.Address.ZipCode
                }
            });

            return Results.Ok(payload);
        });

        group.MapPost("/properties/{id:guid}/approve", async (Guid id, HttpContext httpContext, IPropertyRepository propertyRepository, CancellationToken cancellationToken) =>
        {
            if (!IsAdmin(httpContext.User))
                return Results.Forbid();

            var updated = await propertyRepository.SetApprovalStatus(id, true, cancellationToken);
            return updated ? Results.Ok() : Results.NotFound();
        });

        group.MapPost("/properties/{id:guid}/reject", async (Guid id, HttpContext httpContext, IPropertyRepository propertyRepository, CancellationToken cancellationToken) =>
        {
            if (!IsAdmin(httpContext.User))
                return Results.Forbid();

            var updated = await propertyRepository.SetApprovalStatus(id, false, cancellationToken);
            return updated ? Results.Ok() : Results.NotFound();
        });

        group.MapPost("/properties/{id:guid}/suspend", async (Guid id, HttpContext httpContext, IPropertyRepository propertyRepository, CancellationToken cancellationToken) =>
        {
            if (!IsAdmin(httpContext.User))
                return Results.Forbid();

            var updated = await propertyRepository.SetPropertyActiveStatus(id, false, cancellationToken);
            return updated ? Results.Ok() : Results.NotFound();
        });

        group.MapGet("/bookings", async (HttpContext httpContext, IBookingRepository bookingRepository, CancellationToken cancellationToken) =>
        {
            if (!IsAdmin(httpContext.User))
                return Results.Forbid();

            var bookings = await bookingRepository.GetAllBookings(cancellationToken);
            var payload = bookings.Select(b => new
            {
                b.Id,
                b.PropertyId,
                PropertyName = b.Property.Name,
                b.GuestId,
                GuestEmail = b.Guest.Email,
                b.StartDate,
                b.EndDate,
                b.GuestCount,
                b.TotalPrice,
                b.BookingStatus,
                b.CreatedAt
            });

            return Results.Ok(payload);
        });
    }

    private static bool IsAdmin(ClaimsPrincipal user)
    {
        return user.IsInRole("Admin");
    }
}
