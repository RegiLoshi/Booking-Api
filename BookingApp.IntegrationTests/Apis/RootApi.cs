using System.Net.Http.Json;
using BookingApp.IntegrationTests.Infrastructure;

namespace BookingApp.IntegrationTests.Apis;

public static class RootApi
{
    public static async Task<string> GetWelcomeMessageAsync(CustomWebApplicationFactory factory)
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<RootResponse>()
                      ?? throw new InvalidOperationException("Root response was empty.");
        return payload.Message;
    }

    public static async Task<string> GetHealthStatusAsync(CustomWebApplicationFactory factory)
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<HealthResponse>()
                      ?? throw new InvalidOperationException("Health response was empty.");
        return payload.Status;
    }

    private sealed class RootResponse
    {
        public string Message { get; set; } = string.Empty;
    }

    private sealed class HealthResponse
    {
        public string Status { get; set; } = string.Empty;
    }
}
