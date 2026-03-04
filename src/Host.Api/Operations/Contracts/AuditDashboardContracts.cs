namespace Host.Api.Operations.Contracts;

public sealed record AuditDashboardSummaryDto(
    DateTimeOffset GeneratedAt,
    int WindowHours,
    int SystemErrorCount,
    int FailedLoginCount,
    decimal SessionRevokeRatePercent,
    IReadOnlyList<HourlyMetricDto> FailedLoginTrend,
    IReadOnlyList<TopEventDto> TopCriticalEvents);

public sealed record HourlyMetricDto(
    DateTimeOffset Hour,
    int Count);

public sealed record TopEventDto(
    string EventType,
    int Count);
