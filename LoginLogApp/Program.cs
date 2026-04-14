using LoginLogInfrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterLoginLogInfrastructure(builder.Configuration);

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("Database:InitializeOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<LoginLogDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.MapGet("/", () => Results.Ok(new { message = "Login Log Backend" }));
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
app.MapGet("/ready", async (LoginLogDbContext dbContext) =>
{
    var canConnect = await dbContext.Database.CanConnectAsync();
    return canConnect
        ? Results.Ok(new { status = "Ready" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
});

app.Run();

public partial class Program { }
