namespace Infrastructure.Persistence.Entities;

public sealed class SecurityEventLog
{
    public long Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? EventType { get; set; }
    public string? Severity { get; set; }
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Resource { get; set; }
    public string? Action { get; set; }
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public string? AdditionalData { get; set; }
}
