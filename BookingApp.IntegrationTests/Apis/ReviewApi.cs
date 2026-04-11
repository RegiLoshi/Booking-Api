using System.Net.Http.Json;
using BookingApp.IntegrationTests.Infrastructure;
using BookingApplication.Features.Reviews.CreateReview;

namespace BookingApp.IntegrationTests.Apis;

public static class ReviewApi
{
    public static async Task<Guid> CreateAsync(
        CustomWebApplicationFactory factory,
        TestUserContext user,
        CreateReviewDto dto)
    {
        using var client = factory.CreateAuthenticatedClient(user);
        var response = await client.PostAsJsonAsync("/v1/review/", dto);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<IdResponse>()
                      ?? throw new InvalidOperationException("Review create response was empty.");

        return payload.Id;
    }

    public static async Task<double?> GetAverageAsync(CustomWebApplicationFactory factory, TestUserContext user, Guid propertyId)
    {
        using var client = factory.CreateAuthenticatedClient(user);
        var response = await client.GetAsync($"/v1/review/property/{propertyId}/average");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<AverageRatingResponse>()
                      ?? throw new InvalidOperationException("Average rating response was empty.");

        return payload.AverageRating;
    }

    private sealed class IdResponse
    {
        public Guid Id { get; set; }
    }

    private sealed class AverageRatingResponse
    {
        public Guid PropertyId { get; set; }
        public double? AverageRating { get; set; }
    }
}
