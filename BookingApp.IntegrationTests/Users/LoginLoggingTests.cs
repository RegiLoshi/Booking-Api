using BookingApplication.Abstractions.Contracts.AuthService;
using BookingApplication.Abstractions.Contracts.Logging;
using BookingApplication.Abstractions.Contracts.Repositories;
using BookingApplication.Features.Users.Login;
using BookingDomain.Entities;
using LoginLogContracts;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using UserEntity = BookingDomain.Entities.Users;

namespace BookingApp.IntegrationTests.Users;

public sealed class LoginLoggingTests
{
    [Fact]
    public async Task Login_succeeds_when_event_publisher_is_disabled_or_fails()
    {
        var user = CreateUser("demo@test.com", "Pass123!");
        var handler = new LogInUserCommandHandler(
            new StubUserRepository(user),
            new StubAuthManager(),
            new ThrowingPublisher(),
            NullLogger<LogInUserCommandHandler>.Instance);

        var response = await handler.Handle(new LogInUserCommand
        {
            LogInUserDto = new LogInUserDto
            {
                Email = user.Email,
                Password = "Pass123!"
            }
        }, CancellationToken.None);

        Assert.Equal(user.Id, response.Id);
        Assert.Equal(user.Email, response.Email);
        Assert.Equal("stub-token", response.Token);
    }

    [Fact]
    public async Task Successful_login_publishes_one_event()
    {
        var user = CreateUser("publisher@test.com", "Pass123!");
        var publisher = new RecordingPublisher();
        var handler = new LogInUserCommandHandler(
            new StubUserRepository(user),
            new StubAuthManager(),
            publisher,
            NullLogger<LogInUserCommandHandler>.Instance);

        await handler.Handle(new LogInUserCommand
        {
            LogInUserDto = new LogInUserDto
            {
                Email = user.Email,
                Password = "Pass123!"
            }
        }, CancellationToken.None);

        var evt = Assert.Single(publisher.Messages);
        Assert.Equal(user.Id, evt.UserId);
        Assert.Equal(user.Email, evt.Email);
        Assert.Equal("booking-backend", evt.SourceApp);
        Assert.Equal("user.login.succeeded", evt.EventType);
        Assert.NotEqual(Guid.Empty, evt.EventId);
    }

    [Fact]
    public async Task Failed_login_does_not_publish_event()
    {
        var user = CreateUser("invalid@test.com", "Pass123!");
        var publisher = new RecordingPublisher();
        var handler = new LogInUserCommandHandler(
            new StubUserRepository(user),
            new StubAuthManager(),
            publisher,
            NullLogger<LogInUserCommandHandler>.Instance);

        await Assert.ThrowsAsync<Exception>(() => handler.Handle(new LogInUserCommand
        {
            LogInUserDto = new LogInUserDto
            {
                Email = user.Email,
                Password = "wrong-password"
            }
        }, CancellationToken.None));

        Assert.Empty(publisher.Messages);
    }

    private static UserEntity CreateUser(string email, string password)
    {
        return new UserEntity
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = email,
            Password = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13),
            PhoneNumber = "123456789",
            ProfilePictureUrl = string.Empty,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private sealed class StubUserRepository(UserEntity? user) : IUserRepository
    {
        public Task<UserEntity?> GetUserByEmail(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(user?.Email == email ? user : null);

        public Task<UserEntity?> GetUserById(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(user?.Id == id ? user : null);

        public Task<UserEntity> AddUser(UserEntity user, CancellationToken cancellationToken = default) => Task.FromResult(user);
        public Task<UserEntity> UpdateUser(UserEntity user, CancellationToken cancellationToken = default) => Task.FromResult(user);
        public Task<UserEntity> DeleteUser(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(user!);
        public Task<List<UserEntity>> GetAllUsers(CancellationToken cancellationToken = default) => Task.FromResult(new List<UserEntity>());
        public Task<bool> SetUserActiveStatus(Guid id, bool isActive, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<UserEntity?> GetById(Guid id, CancellationToken cancellationToken = default) => GetUserById(id, cancellationToken);
        public Task<UserEntity> Add(UserEntity entity, CancellationToken cancellationToken = default) => Task.FromResult(entity);
        public Task<UserEntity> Update(UserEntity entity, CancellationToken cancellationToken = default) => Task.FromResult(entity);
        public Task Delete(UserEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class StubAuthManager : IAuthManager
    {
        public string GenerateToken(UserEntity user) => "stub-token";
    }

    private sealed class RecordingPublisher : ILoginEventPublisher
    {
        public List<LoginSucceededLogMessage> Messages { get; } = new();

        public Task PublishAsync(LoginSucceededLogMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingPublisher : ILoginEventPublisher
    {
        public Task PublishAsync(LoginSucceededLogMessage message, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Kafka unavailable");
    }
}
