namespace BookingApp.IntegrationTests.Infrastructure;

public static class ScenarioRunner
{
    public static async Task RunAsync(Func<CustomWebApplicationFactory, Task> scenario)
    {
        await using var factory = new CustomWebApplicationFactory();
        await factory.InitializeAsync();
        await scenario(factory);
    }
}
