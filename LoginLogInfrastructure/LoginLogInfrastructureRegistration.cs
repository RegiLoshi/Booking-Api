using LoginLogInfrastructure.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LoginLogInfrastructure;

public static class LoginLogInfrastructureRegistration
{
    public static IServiceCollection RegisterLoginLogInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<LoginLogDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.Configure<LoginLogKafkaOptions>(configuration.GetSection("Kafka"));
        services.AddScoped<LoginLogMessageProcessor>();
        services.AddHostedService<LoginLogConsumerBackgroundService>();

        return services;
    }
}
