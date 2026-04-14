using System.Text.Json;
using LoginLogContracts;
using LoginLogInfrastructure;
using LoginLogInfrastructure.Kafka;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LoginLogApp.Tests;

public sealed class LoginLogMessageProcessorTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private LoginLogDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        var options = new DbContextOptionsBuilder<LoginLogDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new LoginLogDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task Processor_persists_valid_login_event()
    {
        var processor = new LoginLogMessageProcessor(_dbContext, NullLogger<LoginLogMessageProcessor>.Instance);
        var message = CreateEvent();
        var payload = JsonSerializer.Serialize(message);

        var stored = await processor.ProcessAsync(payload);

        Assert.True(stored);
        var row = await _dbContext.LoginLogs.SingleAsync();
        Assert.Equal(message.EventId, row.EventId);
        Assert.Equal(message.Email, row.Email);
        Assert.Equal(payload, row.PayloadJson);
    }

    [Fact]
    public async Task Processor_ignores_duplicate_event_id()
    {
        var processor = new LoginLogMessageProcessor(_dbContext, NullLogger<LoginLogMessageProcessor>.Instance);
        var message = CreateEvent();
        var payload = JsonSerializer.Serialize(message);

        var first = await processor.ProcessAsync(payload);
        var second = await processor.ProcessAsync(payload);

        Assert.True(first);
        Assert.False(second);
        Assert.Equal(1, await _dbContext.LoginLogs.CountAsync());
    }

    [Fact]
    public async Task Processor_rejects_malformed_payload_without_throwing()
    {
        var processor = new LoginLogMessageProcessor(_dbContext, NullLogger<LoginLogMessageProcessor>.Instance);

        var stored = await processor.ProcessAsync("{not-json");

        Assert.False(stored);
        Assert.Equal(0, await _dbContext.LoginLogs.CountAsync());
    }

    private static LoginSucceededLogMessage CreateEvent()
    {
        return new LoginSucceededLogMessage
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = DateTime.UtcNow,
            UserId = Guid.NewGuid(),
            Email = "logger@test.com",
            SourceApp = "booking-backend",
            EventType = "user.login.succeeded"
        };
    }
}
