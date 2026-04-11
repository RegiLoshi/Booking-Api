using System.Net;
using System.Net.Http.Json;
using BookingApp.IntegrationTests.Infrastructure;
using BookingApplication.Features.Properties.CreateProperty;
using BookingApplication.Features.Properties.GetPropertyDetails;
using BookingApplication.Features.Properties.SearchProperties;
using BookingApplication.Features.Properties.UpdateProperty;

namespace BookingApp.IntegrationTests.Apis;

public static class PropertyApi
{
    public static async Task<Guid> CreateAsync(
        CustomWebApplicationFactory factory,
        TestUserContext user,
        CreatePropertyDto dto)
    {
        using var client = factory.CreateAuthenticatedClient(user);
        var response = await client.PostAsJsonAsync("/v1/property/createProperty", dto);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<IdResponse>()
                      ?? throw new InvalidOperationException("Property create response was empty.");

        return payload.Id;
    }

    public static async Task UpdateAsync(
        CustomWebApplicationFactory factory,
        TestUserContext user,
        Guid propertyId,
        UpdatePropertyDto dto)
    {
        using var client = factory.CreateAuthenticatedClient(user);
        var response = await client.PutAsJsonAsync($"/v1/property/{propertyId}", dto);
        response.EnsureSuccessStatusCode();
    }

    public static async Task<PropertyDetailsDto?> GetDetailsAsync(CustomWebApplicationFactory factory, Guid propertyId)
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync($"/v1/property/{propertyId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PropertyDetailsDto>();
    }

    public static async Task<HttpStatusCode> GetDetailsStatusAsync(CustomWebApplicationFactory factory, Guid propertyId)
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync($"/v1/property/{propertyId}");
        return response.StatusCode;
    }

    public static async Task<SearchPropertiesResponse> SearchAsync(CustomWebApplicationFactory factory, string url)
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Unexpected status {(int)response.StatusCode} ({response.StatusCode}) from search endpoint. Body: {body}");
        }

        return await response.Content.ReadFromJsonAsync<SearchPropertiesResponse>()
               ?? throw new InvalidOperationException("Search response was empty.");
    }

    public static async Task<HttpStatusCode> SearchStatusAsync(CustomWebApplicationFactory factory, string url)
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync(url);
        return response.StatusCode;
    }

    private sealed class IdResponse
    {
        public Guid Id { get; set; }
    }
}
