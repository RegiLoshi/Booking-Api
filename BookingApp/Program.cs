using BookingApp.Endpoints;
using BookingApp.Middleware;
using BookingInfrastructure;
using BookingApplication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterInfrastructure(builder.Configuration);
builder.Services.RegisterApplication();
builder.Services.ConfigureJWT(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddScoped<BookingStatusEmailMiddleware>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<BookingStatusEmailMiddleware>();
app.UseMiddleware<GlobalExceptionHandler>();

app.MapRootEndpoints();
app.MapUserEndpoints();
app.MapPropertyEndpoints();
app.MapBookingEndpoints();
app.MapReviewEndpoints();

app.Run();

public partial class Program { }
