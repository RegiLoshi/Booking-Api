namespace LoginLogContracts;

public class LoginSucceededLogMessage
{
    public Guid EventId { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string SourceApp { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
}
