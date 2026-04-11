using BookingApp.Endpoints;
using BookingApp.Hubs;
using BookingApp.Middleware;
using BookingApp.SignalR;
using BookingInfrastructure;
using BookingApplication;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterInfrastructure(builder.Configuration);
builder.Services.RegisterApplication();
builder.Services.ConfigureJWT(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, UserIdFromClaimProvider>();
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
app.MapAdminEndpoints();
app.MapHub<BookingHub>("/hubs/booking");

app.Run();

public partial class Program { }
