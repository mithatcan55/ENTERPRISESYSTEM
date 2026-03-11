namespace Infrastructure.Persistence.Entities;

public sealed class SystemLog
{
    public long Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? TimeZone { get; set; }
    public string? Level { get; set; }
    public string? Category { get; set; }
    public string? Source { get; set; }
    public string? Message { get; set; }
    public string? MessageTemplate { get; set; }
    public string? Exception { get; set; }
    public string? StackTrace { get; set; }
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }
    public string? RequestId { get; set; }
    public string? SessionId { get; set; }
    public string? TenantId { get; set; }
    public string? HttpMethod { get; set; }
    public string? HttpPath { get; set; }
    public string? QueryString { get; set; }
    public int? HttpStatusCode { get; set; }
    public long? HttpDurationMs { get; set; }
    public string? RequestBody { get; set; }
    public string? ResponseBody { get; set; }
    public string? RequestHeaders { get; set; }
    public string? ResponseHeaders { get; set; }
    public string? DbOperation { get; set; }
    public string? DbTable { get; set; }
    public string? DbCommand { get; set; }
    public string? DbParameters { get; set; }
    public long? DbDurationMs { get; set; }
    public int? DbRowsAffected { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? EntityAction { get; set; }
    public string? OperationName { get; set; }
    public long? DurationMs { get; set; }
    public long? MemoryUsedBytes { get; set; }
    public string? MachineName { get; set; }
    public string? Environment { get; set; }
    public string? ApplicationName { get; set; }
    public string? ApplicationVersion { get; set; }
    public int? ProcessId { get; set; }
    public int? ThreadId { get; set; }
    public string? Properties { get; set; }
}
