using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;

namespace BookingApplication;

public static class ApplicationServicesRegistration
{
    public static IServiceCollection RegisterApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        return services;
    }
}