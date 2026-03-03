namespace Infrastructure.Persistence.Entities;

public sealed class PerformanceLog
{
    public long Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? CorrelationId { get; set; }
    public string? UserId { get; set; }
    public string? OperationName { get; set; }
    public string? OperationType { get; set; }
    public long DurationMs { get; set; }
    public long MemoryBefore { get; set; }
    public long MemoryAfter { get; set; }
    public long MemoryUsed { get; set; }
    public bool IsSlowOperation { get; set; }
    public long ThresholdMs { get; set; }
    public string? AdditionalData { get; set; }
}
