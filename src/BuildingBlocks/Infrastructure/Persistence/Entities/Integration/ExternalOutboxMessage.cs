namespace Infrastructure.Persistence.Entities.Integration;

public sealed class ExternalOutboxMessage
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; } = 5;
    public DateTime NextAttemptAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? LastError { get; set; }
    public string? CorrelationId { get; set; }
    public string? DeduplicationKey { get; set; }
}
