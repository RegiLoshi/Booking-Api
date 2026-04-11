using BookingApp.IntegrationTests.Infrastructure;
using BookingInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BookingApp.IntegrationTests.Seeders;

public static class ReviewSeeder
{
    public static async Task<Guid> CreateAsync(
        CustomWebApplicationFactory factory,
        Guid bookingId,
        int rating,
        string comment)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();

        var review = new BookingDomain.Entities.Reviews
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            GuestId = factory.Client.Id,
            Rating = rating,
            Comment = comment,
            CreatedAt = DateTime.UtcNow
        };

        db.Reviews.Add(review);
        await db.SaveChangesAsync();
        return review.Id;
    }
}
