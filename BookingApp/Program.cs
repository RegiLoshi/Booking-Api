using BookingInfrastructure;
using BookingApplication;
using BookingApplication.Features.Users.Register;
using BookingApplication.Features.Users.Login;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterInfrastructure(builder.Configuration);
builder.Services.RegisterApplication();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });
builder.Services.AddAuthorization();
var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { message = "Welcome to Booking API" }));

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

   app.MapPost("v1/user/register", async (RegisterUserCommand command, IMediator mediator) =>
   {
       try 
       {
           var result = await mediator.Send(command);
           return Results.Ok(new { id = result });
       }
       catch (ValidationException ex)
       {
           return Results.BadRequest(ex.Errors);
       }
   });

app.MapPost("v1/user/login", async (LogInUserCommand command, IMediator mediator) =>
{
    var result = await mediator.Send(command);
    return Results.Ok(result);
});
app.Run();