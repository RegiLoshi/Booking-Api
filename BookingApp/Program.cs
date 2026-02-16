using BookingInfrastructure;
using BookingApplication;
using BookingApplication.Features.Users.Register;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.RegisterInfrastructure(builder.Configuration);
builder.Services.RegisterApplication();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/", () => Results.Ok(new { message = "Welcome to Booking API" }))
    .WithName("GetRoot")
    .WithTags("Health");

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
    .WithName("GetHealth")
    .WithTags("Health");

app.MapPost("/users/register", async (RegisterUserCommand command, IMediator mediator) =>
{
    var result = await mediator.Send(command);
    return Results.Ok(new { id = result });
})
.WithName("RegisterUser")
.WithTags("Users");

app.Run();