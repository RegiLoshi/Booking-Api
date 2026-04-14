using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Primitives;

namespace BookingInfrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using BookingDomain.Repositories;
using BookingInfrastructure.Repositories;
using BookingInfrastructure.Contracts.Repositories;
using BookingApplication.Abstractions.Contracts.Repositories;
using BookingApplication.Abstractions.Contracts.AuthService;
using BookingApplication.Abstractions.Contracts.Email;
using BookingApplication.Abstractions.Contracts.Logging;
using BookingInfrastructure.Contracts.AuthService;
using BookingInfrastructure.Contracts.Email;
using BookingInfrastructure.Logging;

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
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IAuthManager, AuthManager>();
        services.Configure<BookingKafkaOptions>(configuration.GetSection("Kafka"));
        services.AddScoped<ILoginEventPublisher>(sp =>
        {
            var options = sp.GetRequiredService<IConfiguration>().GetSection("Kafka").Get<BookingKafkaOptions>() ?? new BookingKafkaOptions();
            return options.ProducerEnabled
                ? sp.GetRequiredService<KafkaLoginEventPublisher>()
                : sp.GetRequiredService<NoOpLoginEventPublisher>();
        });
        services.AddScoped<KafkaLoginEventPublisher>();
        services.AddScoped<NoOpLoginEventPublisher>();

        services.Configure<SendGridOptions>(configuration.GetSection("SendGrid"));
        services.AddScoped<IEmailSender, SendGridEmailSender>();
        
        return services;
    }

    public static IServiceCollection ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtConfig");
        var secretKey = jwtSettings.GetSection("SecretKey").Value;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
                };


                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var path = context.HttpContext.Request.Path;
                        if (path.StartsWithSegments("/hubs/booking"))
                        {
                            if (context.HttpContext.Request.Query.TryGetValue("access_token", out StringValues token) &&
                                !StringValues.IsNullOrEmpty(token))
                            {
                                context.Token = token.ToString();
                            }
                        }

                        return Task.CompletedTask;
                    }
                };
            }
        );
        return services;
    }
}
