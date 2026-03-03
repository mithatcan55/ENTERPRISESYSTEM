namespace Host.Api.Operations.Contracts;

public sealed class LogQueryRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public DateTimeOffset? StartAt { get; set; }
    public DateTimeOffset? EndAt { get; set; }
    public string? CorrelationId { get; set; }
    public string? Search { get; set; }
}

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record SystemLogListItemDto(
    long Id,
    DateTimeOffset Timestamp,
    string? Level,
    string? Category,
    string? Source,
    string? Message,
    string? CorrelationId,
    int? HttpStatusCode,
    string? UserId,
    string? Username);

public sealed record SecurityEventListItemDto(
    long Id,
    DateTimeOffset Timestamp,
    string? EventType,
    string? Severity,
    string? UserId,
    string? Username,
    string? Resource,
    string? Action,
    bool IsSuccessful,
    string? FailureReason,
    string? IpAddress);

public sealed record HttpRequestLogListItemDto(
    long Id,
    DateTimeOffset Timestamp,
    string? Method,
    string? Path,
    int StatusCode,
    long DurationMs,
    string? CorrelationId,
    bool IsError,
    string? UserId,
    string? Username,
    string? IpAddress);
