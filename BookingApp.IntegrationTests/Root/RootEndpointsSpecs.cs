using BookingApp.IntegrationTests.Infrastructure;
using Xunit;

namespace BookingApp.IntegrationTests.Root;

[Collection(PropertyApiCollection.Name)]
public sealed class RootEndpointsSpecs
{
    [Fact]
    public Task Root_endpoint_returns_welcome_message()
        => RootEndpointsScenarios.Root_endpoint_returns_welcome_message();

    [Fact]
    public Task Health_endpoint_returns_healthy_status()
        => RootEndpointsScenarios.Health_endpoint_returns_healthy_status();
}
