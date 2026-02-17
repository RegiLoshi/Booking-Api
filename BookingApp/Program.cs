using BookingInfrastructure;
using BookingApplication;
using BookingApplication.Features.Users.Register;
using FluentValidation;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterInfrastructure(builder.Configuration);
builder.Services.RegisterApplication();

var app = builder.Build();

app.UseHttpsRedirection();


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

app.Run();