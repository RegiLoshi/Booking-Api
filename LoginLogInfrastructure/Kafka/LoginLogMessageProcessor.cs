using System.Text.Json;
using LoginLogContracts;
using LoginLogInfrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LoginLogInfrastructure.Kafka;

public class LoginLogMessageProcessor(
    LoginLogDbContext dbContext,
    ILogger<LoginLogMessageProcessor> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<bool> ProcessAsync(string payloadJson, CancellationToken cancellationToken = default)
    {
        LoginSucceededLogMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<LoginSucceededLogMessage>(payloadJson, JsonOptions);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Received malformed login log message");
            return false;
        }

        if (message == null || message.EventId == Guid.Empty || message.UserId == Guid.Empty || string.IsNullOrWhiteSpace(message.Email))
        {
            logger.LogWarning("Received invalid login log payload");
            return false;
        }

        var entry = new LoginLogEntry
        {
            EventId = message.EventId,
            OccurredAtUtc = message.OccurredAtUtc,
            ReceivedAtUtc = DateTime.UtcNow,
            EventType = message.EventType,
            SourceApp = message.SourceApp,
            UserId = message.UserId,
            Email = message.Email,
            PayloadJson = payloadJson
        };

        dbContext.LoginLogs.Add(entry);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) when (IsDuplicateEvent(ex))
        {
            logger.LogInformation("Ignoring duplicate login log event {EventId}", message.EventId);
            return false;
        }
    }

    private static bool IsDuplicateEvent(DbUpdateException ex)
    {
        var text = ex.InnerException?.Message ?? ex.Message;
        return text.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
               || text.Contains("unique", StringComparison.OrdinalIgnoreCase);
    }
}
