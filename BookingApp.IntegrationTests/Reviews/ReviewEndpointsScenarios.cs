using BookingApp.IntegrationTests.Apis;
using BookingApp.IntegrationTests.Infrastructure;
using BookingApp.IntegrationTests.Seeders;
using BookingApplication.Features.Reviews.CreateReview;
using BookingDomain.Entities;
using BookingInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BookingApp.IntegrationTests.Reviews;

public static class ReviewEndpointsScenarios
{
    public static async Task Guest_can_create_review_for_completed_booking()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var propertyId = await PropertySeeder.CreateAsync(factory, name: $"Review Property {Guid.NewGuid():N}", pricePerDay: 100m, cleaningFeePerDay: 10m, amenities: new List<Amenity> { Amenity.WiFi });
            var bookingId = await BookingSeeder.CreateAsync(factory, propertyId, status: BookingStatus.Completed, startDate: DateTime.UtcNow.Date.AddDays(-10), endDate: DateTime.UtcNow.Date.AddDays(-8), cleaningFee: 20m, priceForPeriod: 200m, totalPrice: 220m);

            var reviewId = await ReviewApi.CreateAsync(factory, factory.Client, new CreateReviewDto
            {
                BookingId = bookingId,
                Rating = 5,
                Comment = "Amazing stay"
            });

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
            var review = await db.Reviews.FindAsync(reviewId);

            Assert.NotNull(review);
            Assert.Equal(bookingId, review!.BookingId);
            Assert.Equal(factory.Client.Id, review.GuestId);
            Assert.Equal(5, review.Rating);
            Assert.Equal("Amazing stay", review.Comment);
        });
    }

    public static async Task Property_average_rating_is_returned_for_reviews()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var propertyId = await PropertySeeder.CreateAsync(factory, name: $"Review Property {Guid.NewGuid():N}", pricePerDay: 100m, cleaningFeePerDay: 10m, amenities: new List<Amenity> { Amenity.WiFi });
            var bookingOne = await BookingSeeder.CreateAsync(factory, propertyId, status: BookingStatus.Completed, startDate: DateTime.UtcNow.Date.AddDays(-10), endDate: DateTime.UtcNow.Date.AddDays(-8), cleaningFee: 20m, priceForPeriod: 200m, totalPrice: 220m);
            var bookingTwo = await BookingSeeder.CreateAsync(factory, propertyId, status: BookingStatus.Completed, startDate: DateTime.UtcNow.Date.AddDays(-20), endDate: DateTime.UtcNow.Date.AddDays(-18), cleaningFee: 20m, priceForPeriod: 200m, totalPrice: 220m);

            await ReviewSeeder.CreateAsync(factory, bookingOne, 4, "Very good");
            await ReviewSeeder.CreateAsync(factory, bookingTwo, 5, "Excellent");

            var average = await ReviewApi.GetAverageAsync(factory, factory.Client, propertyId);

            Assert.Equal(4.5d, average);
        });
    }
}
