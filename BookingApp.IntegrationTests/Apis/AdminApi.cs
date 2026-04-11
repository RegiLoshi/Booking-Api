using System.Net.Http.Json;
using BookingApp.IntegrationTests.Infrastructure;

namespace BookingApp.IntegrationTests.Apis;

public static class AdminApi
{
    public static async Task<List<T>> GetUsersAsync<T>(CustomWebApplicationFactory factory)
    {
        using var client = factory.CreateAuthenticatedClient(factory.Admin);
        var response = await client.GetAsync("/v1/admin/users");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<T>>() ?? new List<T>();
    }

    public static async Task SuspendUserAsync(CustomWebApplicationFactory factory, Guid userId)
    {
        using var client = factory.CreateAuthenticatedClient(factory.Admin);
        var response = await client.PostAsync($"/v1/admin/users/{userId}/suspend", content: null);
        response.EnsureSuccessStatusCode();
    }

    public static async Task DeleteUserAsync(CustomWebApplicationFactory factory, Guid userId)
    {
        using var client = factory.CreateAuthenticatedClient(factory.Admin);
        var response = await client.DeleteAsync($"/v1/admin/users/{userId}");
        response.EnsureSuccessStatusCode();
    }

    public static async Task<List<T>> GetPropertiesAsync<T>(CustomWebApplicationFactory factory)
    {
        using var client = factory.CreateAuthenticatedClient(factory.Admin);
        var response = await client.GetAsync("/v1/admin/properties");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<T>>() ?? new List<T>();
    }

    public static async Task ApprovePropertyAsync(CustomWebApplicationFactory factory, Guid propertyId)
    {
        using var client = factory.CreateAuthenticatedClient(factory.Admin);
        var response = await client.PostAsync($"/v1/admin/properties/{propertyId}/approve", content: null);
        response.EnsureSuccessStatusCode();
    }

    public static async Task RejectPropertyAsync(CustomWebApplicationFactory factory, Guid propertyId)
    {
        using var client = factory.CreateAuthenticatedClient(factory.Admin);
        var response = await client.PostAsync($"/v1/admin/properties/{propertyId}/reject", content: null);
        response.EnsureSuccessStatusCode();
    }

    public static async Task SuspendPropertyAsync(CustomWebApplicationFactory factory, Guid propertyId)
    {
        using var client = factory.CreateAuthenticatedClient(factory.Admin);
        var response = await client.PostAsync($"/v1/admin/properties/{propertyId}/suspend", content: null);
        response.EnsureSuccessStatusCode();
    }

    public static async Task<List<T>> GetBookingsAsync<T>(CustomWebApplicationFactory factory)
    {
        using var client = factory.CreateAuthenticatedClient(factory.Admin);
        var response = await client.GetAsync("/v1/admin/bookings");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<T>>() ?? new List<T>();
    }
}
