namespace Infrastructure.Persistence.Entities;

public sealed class HttpRequestLog
{
    public long Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? CorrelationId { get; set; }
    public string? Method { get; set; }
    public string? Path { get; set; }
    public string? QueryString { get; set; }
    public string? RequestHeaders { get; set; }
    public string? RequestBody { get; set; }
    public long RequestSize { get; set; }
    public int StatusCode { get; set; }
    public string? ResponseHeaders { get; set; }
    public string? ResponseBody { get; set; }
    public long ResponseSize { get; set; }
    public long DurationMs { get; set; }
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsError { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorStackTrace { get; set; }
}
