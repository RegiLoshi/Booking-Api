namespace BookingApp.Endpoints;

public static class RootEndpoints
{
    public static void MapRootEndpoints(this WebApplication app)
    {
        app.MapGet("/", () => Results.Ok(new { message = "Welcome to Booking API" }));
        app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
    }
}
