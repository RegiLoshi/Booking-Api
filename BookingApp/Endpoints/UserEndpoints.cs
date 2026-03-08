using BookingApplication.Features.Users.ChangePassword;
using BookingApplication.Features.Users.Login;
using BookingApplication.Features.Users.Register;
using BookingApplication.Features.Users.UpdateUser;
using FluentValidation;
using MediatR;

namespace BookingApp.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("v1/user");

        group.MapPost("/register", async (RegisterUserCommand command, IMediator mediator) =>
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

        group.MapPost("/login", async (LogInUserCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(result);
        });

        group.MapPost("/update", async (UpdateUserCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(result);
        });

        group.MapPost("/changePassword", async (UserChangePasswordCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(result);
        });
    }
}
