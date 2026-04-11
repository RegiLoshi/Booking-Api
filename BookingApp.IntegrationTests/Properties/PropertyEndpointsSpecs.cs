using BookingApp.IntegrationTests.Infrastructure;
using Xunit;

namespace BookingApp.IntegrationTests.Properties;

[Collection(PropertyApiCollection.Name)]
public sealed class PropertyEndpointsSpecs
{
    [Fact]
    public Task Owner_can_create_property()
        => PropertyEndpointsScenarios.Owner_can_create_property();

    [Fact]
    public Task Owner_can_update_property()
        => PropertyEndpointsScenarios.Owner_can_update_property();

    [Fact]
    public Task Public_can_get_property_details_for_visible_property()
        => PropertyEndpointsScenarios.Public_can_get_property_details_for_visible_property();

    [Fact]
    public Task Public_get_property_details_returns_not_found_for_hidden_property()
        => PropertyEndpointsScenarios.Public_get_property_details_returns_not_found_for_hidden_property();
}
