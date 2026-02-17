using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;
using BookingApplication.Validator;
using FluentValidation;

namespace BookingApplication;

public static class ApplicationServicesRegistration
{
    public static IServiceCollection RegisterApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }
}
