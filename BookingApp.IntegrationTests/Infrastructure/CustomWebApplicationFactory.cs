using System.Net.Http.Json;
using System.Text.Json;
using BookingApplication.Features.Users.Login;
using BookingApplication.Features.Users.Register;
using BookingApplication.Abstractions.Contracts.AuthService;
using BookingApplication.Abstractions.Contracts.Repositories;
using BookingDomain.Entities;
using BookingInfrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace BookingApp.IntegrationTests.Infrastructure;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private readonly WebApplicationFactoryClientOptions _clientOptions = new()
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false
    };

    public TestUserContext Owner { get; private set; } = null!;
    public TestUserContext Client { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
                ["JwtConfig:SecretKey"] = "integration-tests-secret-key-012345678901234567890123456789",
                ["JwtConfig:Lifetime"] = "3600",
                ["SendGrid:ApiKey"] = "",
                ["SendGrid:FromEmail"] = "",
                ["SendGrid:FromName"] = "Tests"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<BookingDbContext>>();
            services.RemoveAll<BookingDbContext>();

            services.AddSingleton(_connection);
            services.AddDbContext<BookingDbContext>((sp, options) =>
            {
                var connection = sp.GetRequiredService<SqliteConnection>();
                options.UseSqlite(connection);
            });

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();

        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        Owner = await RegisterAndLoginAsync("owner");
        Client = await RegisterAndLoginAsync("client");
    }

    public new async Task DisposeAsync()
    {
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
            await DeleteTempUsersAsync(db);
            await db.Database.EnsureDeletedAsync();
        }

        await _connection.DisposeAsync();
        await base.DisposeAsync();
    }

    public HttpClient CreateAuthenticatedClient(TestUserContext user)
    {
        var client = CreateClient(_clientOptions);
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, user.Id.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.EmailHeader, user.Email);
        return client;
    }

    public new HttpClient CreateClient()
    {
        return base.CreateClient(_clientOptions);
    }

    public async Task ApprovePropertyAsync(Guid propertyId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();

        var property = await db.Properties.FirstAsync(p => p.Id == propertyId);
        property.IsApproved = true;
        await db.SaveChangesAsync();
    }

    private async Task<TestUserContext> RegisterAndLoginAsync(string label)
    {
        var suffix = Guid.NewGuid().ToString("N");
        var email = $"{label}.{suffix}@integration.test";
        const string password = "Pass123!";

        using var client = CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/v1/user/register", new RegisterUserCommand
        {
            CreateUserDto = new CreateUserDto
            {
                FirstName = label,
                LastName = "test",
                Email = email,
                Password = password,
                PhoneNumber = "1234567890",
                ProfilePictureUrl = string.Empty
            }
        });
        registerResponse.EnsureSuccessStatusCode();

        using var scope = Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var authManager = scope.ServiceProvider.GetRequiredService<IAuthManager>();

        var user = await userRepository.GetUserByEmail(email)
                   ?? throw new InvalidOperationException($"Registered test user {email} was not found.");

        return new TestUserContext
        {
            Id = user.Id,
            Email = email,
            Password = password,
            Token = authManager.GenerateToken(user)
        };
    }

    private async Task DeleteTempUsersAsync(BookingDbContext db)
    {
        var tempUserIds = new[] { Owner?.Id, Client?.Id }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();

        if (tempUserIds.Count == 0)
            return;

        var reviews = await db.Reviews.Where(r => tempUserIds.Contains(r.GuestId)).ToListAsync();
        db.Reviews.RemoveRange(reviews);

        var bookings = await db.Bookings.Where(b => tempUserIds.Contains(b.GuestId)).ToListAsync();
        db.Bookings.RemoveRange(bookings);

        var properties = await db.Properties.Where(p => tempUserIds.Contains(p.OwnerId)).ToListAsync();
        var addressIds = properties.Select(p => p.AddressId).Distinct().ToList();
        db.Properties.RemoveRange(properties);

        var ownerProfiles = await db.OwnerProfiles.Where(op => tempUserIds.Contains(op.UserId)).ToListAsync();
        db.OwnerProfiles.RemoveRange(ownerProfiles);

        var userRoles = await db.UserRoles.Where(ur => tempUserIds.Contains(ur.UserId)).ToListAsync();
        db.UserRoles.RemoveRange(userRoles);

        await db.SaveChangesAsync();

        var addresses = await db.Addresses
            .Where(a => addressIds.Contains(a.Id) && !db.Properties.Any(p => p.AddressId == a.Id))
            .ToListAsync();
        db.Addresses.RemoveRange(addresses);

        var users = await db.Users.Where(u => tempUserIds.Contains(u.Id)).ToListAsync();
        db.Users.RemoveRange(users);

        await db.SaveChangesAsync();
    }
}
