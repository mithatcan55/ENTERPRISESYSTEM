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
