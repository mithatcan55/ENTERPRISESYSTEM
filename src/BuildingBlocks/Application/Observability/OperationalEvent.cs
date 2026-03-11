namespace Application.Observability;

public sealed class OperationalEvent
{
    public string EventName { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string Severity { get; set; } = "Information";
    public string Message { get; set; } = string.Empty;
    public string? Source { get; set; }
    public string? OperationName { get; set; }
    public bool IsSuccessful { get; set; } = true;
    public string? FailureReason { get; set; }
    public string? CorrelationId { get; set; }
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? HttpMethod { get; set; }
    public string? HttpPath { get; set; }
    public int? HttpStatusCode { get; set; }
    public long? DurationMs { get; set; }
    public string? Resource { get; set; }
    public string? Action { get; set; }
    public string? ExceptionMessage { get; set; }
    public string? StackTrace { get; set; }
    public Dictionary<string, object?> Properties { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
