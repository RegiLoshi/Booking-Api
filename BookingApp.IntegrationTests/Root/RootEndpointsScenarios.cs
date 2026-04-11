using BookingApp.IntegrationTests.Apis;
using BookingApp.IntegrationTests.Infrastructure;
using Xunit;

namespace BookingApp.IntegrationTests.Root;

public static class RootEndpointsScenarios
{
    public static async Task Root_endpoint_returns_welcome_message()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var message = await RootApi.GetWelcomeMessageAsync(factory);
            Assert.Equal("Welcome to Booking API", message);
        });
    }

    public static async Task Health_endpoint_returns_healthy_status()
    {
        await ScenarioRunner.RunAsync(async factory =>
        {
            var status = await RootApi.GetHealthStatusAsync(factory);
            Assert.Equal("Healthy", status);
        });
    }
}
