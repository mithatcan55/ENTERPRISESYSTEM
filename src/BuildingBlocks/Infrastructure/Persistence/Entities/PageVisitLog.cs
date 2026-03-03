namespace Infrastructure.Persistence.Entities;

public sealed class PageVisitLog
{
    public long Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public string? FullName { get; set; }
    public string? Department { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? PagePath { get; set; }
    public string? PageTitle { get; set; }
    public string? PageCategory { get; set; }
    public string? QueryString { get; set; }
    public string? Referrer { get; set; }
    public string? SessionId { get; set; }
    public string? CorrelationId { get; set; }
    public long VisitDurationMs { get; set; }
    public string? Browser { get; set; }
    public string? BrowserVersion { get; set; }
    public string? OperatingSystem { get; set; }
    public string? DeviceType { get; set; }
    public bool IsMobile { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? AdditionalData { get; set; }
}
