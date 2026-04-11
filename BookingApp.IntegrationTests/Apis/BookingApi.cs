using System.Net.Http.Json;
using BookingApp.IntegrationTests.Infrastructure;
using BookingApplication.Features.Booking.CreateBooking;

namespace BookingApp.IntegrationTests.Apis;

public static class BookingApi
{
    public static async Task<Guid> CreateAsync(
        CustomWebApplicationFactory factory,
        TestUserContext user,
        CreateBookingDto dto)
    {
        using var client = factory.CreateAuthenticatedClient(user);
        var response = await client.PostAsJsonAsync("/v1/booking/", dto);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<IdResponse>()
                      ?? throw new InvalidOperationException("Booking create response was empty.");

        return payload.Id;
    }

    public static async Task ConfirmAsync(CustomWebApplicationFactory factory, TestUserContext user, Guid bookingId)
    {
        using var client = factory.CreateAuthenticatedClient(user);
        var response = await client.PostAsync($"/v1/booking/{bookingId}/confirm", content: null);
        response.EnsureSuccessStatusCode();
    }

    public static async Task RejectAsync(CustomWebApplicationFactory factory, TestUserContext user, Guid bookingId)
    {
        using var client = factory.CreateAuthenticatedClient(user);
        var response = await client.PostAsync($"/v1/booking/{bookingId}/reject", content: null);
        response.EnsureSuccessStatusCode();
    }

    public static async Task CancelAsync(CustomWebApplicationFactory factory, TestUserContext user, Guid bookingId)
    {
        using var client = factory.CreateAuthenticatedClient(user);
        var response = await client.PostAsync($"/v1/booking/{bookingId}/cancel", content: null);
        response.EnsureSuccessStatusCode();
    }

    public static async Task CompleteAsync(CustomWebApplicationFactory factory, TestUserContext user, Guid bookingId)
    {
        using var client = factory.CreateAuthenticatedClient(user);
        var response = await client.PostAsync($"/v1/booking/{bookingId}/complete", content: null);
        response.EnsureSuccessStatusCode();
    }

    private sealed class IdResponse
    {
        public Guid Id { get; set; }
    }
}
