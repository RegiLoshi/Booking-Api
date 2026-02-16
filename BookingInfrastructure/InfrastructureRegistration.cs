namespace BookingInfrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

public static class InfrastructureRegistration
{
    public static IServiceCollection RegisterInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<BookingDbContext>(options =>
            options.UseSqlServer(connectionString));
        
        return services;
    }
}

