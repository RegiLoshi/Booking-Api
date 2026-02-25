namespace BookingInfrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using BookingDomain.Repositories;
using BookingInfrastructure.Repositories;
using BookingInfrastructure.Contracts.Repositories;
using BookingApplication.Abstractions.Contracts.Repositories;

public static class InfrastructureRegistration
{
    public static IServiceCollection RegisterInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<BookingDbContext>(options =>
            options.UseSqlServer(connectionString));
        
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        
        return services;
    }
}

