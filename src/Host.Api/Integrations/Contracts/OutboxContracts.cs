namespace Host.Api.Integrations.Contracts;

public static class OutboxEventTypes
{
    public const string MailNotification = "mail.notification";
    public const string ExcelReport = "excel.report";
}

public sealed class QueueMailRequest
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public sealed class QueueExcelReportRequest
{
    public string ReportName { get; set; } = string.Empty;
    public IReadOnlyList<string> Headers { get; set; } = Array.Empty<string>();
    public IReadOnlyList<IReadOnlyList<string>> Rows { get; set; } = Array.Empty<IReadOnlyList<string>>();
    public string? NotifyEmail { get; set; }
}

public sealed record OutboxMessageQueuedDto(long Id, string EventType, string Status, DateTime CreatedAt);

public sealed class OutboxMessageQueryRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? Status { get; set; }
    public string? EventType { get; set; }
    public string? Search { get; set; }
}

public sealed record OutboxMessageListItemDto(
    long Id,
    DateTime CreatedAt,
    string EventType,
    string Status,
    int AttemptCount,
    int MaxAttempts,
    DateTime NextAttemptAt,
    DateTime? ProcessedAt,
    string? LastError,
    string? CorrelationId,
    string? DeduplicationKey);

public sealed record OutboxPagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed class MailOutboxPayload
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public sealed class ExcelOutboxPayload
{
    public string ReportName { get; set; } = string.Empty;
    public IReadOnlyList<string> Headers { get; set; } = Array.Empty<string>();
    public IReadOnlyList<IReadOnlyList<string>> Rows { get; set; } = Array.Empty<IReadOnlyList<string>>();
    public string? NotifyEmail { get; set; }
}
