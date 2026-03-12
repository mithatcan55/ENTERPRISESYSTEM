using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Application.Observability;
using Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Options;

namespace Infrastructure.Observability;

public sealed class OperationalEventPublisher(
    ILogEventWriter logEventWriter,
    IEnumerable<INotificationChannel> notificationChannels,
    IOptions<ObservabilityRoutingOptions> routingOptions) : IOperationalEventPublisher
{
    private static readonly string[] SeverityOrder = ["Trace", "Debug", "Information", "Warning", "Error", "Critical"];
    private readonly IReadOnlyDictionary<string, INotificationChannel> _channels = notificationChannels.ToDictionary(x => x.ChannelName, StringComparer.OrdinalIgnoreCase);

    public async Task PublishAsync(OperationalEvent operationalEvent, CancellationToken cancellationToken = default)
    {
        // Publisher'in ana isi event'i "nereye gidecek?" sorusuna gore route etmektir.
        // Handler veya middleware bununla ilgilenmez; sadece event uretir.
        var route = ResolveRoute(operationalEvent);
        if (route is null)
        {
            return;
        }

        if (route.WriteSystemLog)
        {
            await logEventWriter.WriteSystemAsync(MapSystemLog(operationalEvent), cancellationToken);
        }

        if (route.WriteSecurityLog)
        {
            await logEventWriter.WriteSecurityAsync(MapSecurityLog(operationalEvent), cancellationToken);
        }

        var performanceThresholdMs = route.PerformanceThresholdMs ?? routingOptions.Value.DefaultPerformanceThresholdMs;

        if (route.WritePerformanceLog && operationalEvent.DurationMs.HasValue)
        {
            await logEventWriter.WritePerformanceAsync(MapPerformanceLog(operationalEvent, performanceThresholdMs), cancellationToken);
        }

        if (route.SendNotification && !routingOptions.Value.DisableAllNotifications)
        {
            var notification = MapNotification(operationalEvent);
            var targetChannels = route.NotificationChannels.Count == 0
                ? _channels.Keys.ToList()
                : route.NotificationChannels;

            foreach (var channelName in targetChannels)
            {
                if (_channels.TryGetValue(channelName, out var channel))
                {
                    notification.Channel = channel.ChannelName;
                    await channel.PublishAsync(notification, cancellationToken);
                }
            }
        }
    }

    private OperationalEventRouteOptions? ResolveRoute(OperationalEvent operationalEvent)
    {
        // Route'lar ozelden genele dogru uygulanir.
        // En sonda '*' varsa fallback route gibi davranir.
        var routes = routingOptions.Value.Routes
            .OrderBy(x => string.Equals(x.EventName, "*", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
            .ToList();

        foreach (var route in routes)
        {
            if (!Matches(route.EventName, operationalEvent.EventName)
                || !Matches(route.Category, operationalEvent.Category)
                || !Matches(route.Source, operationalEvent.Source)
                || !Matches(route.OperationName, operationalEvent.OperationName))
            {
                continue;
            }

            if (route.MinimumSeverities.Count > 0 && !MeetsSeverity(route.MinimumSeverities, operationalEvent.Severity))
            {
                continue;
            }

            if (route.OnlyFailures && operationalEvent.IsSuccessful != false)
            {
                continue;
            }

            if (route.OnlySuccess && operationalEvent.IsSuccessful != true)
            {
                continue;
            }

            if (route.MinimumHttpStatusCode.HasValue && (!operationalEvent.HttpStatusCode.HasValue || operationalEvent.HttpStatusCode.Value < route.MinimumHttpStatusCode.Value))
            {
                continue;
            }

            if (route.MaximumHttpStatusCode.HasValue && (!operationalEvent.HttpStatusCode.HasValue || operationalEvent.HttpStatusCode.Value > route.MaximumHttpStatusCode.Value))
            {
                continue;
            }

            return route;
        }

        return null;
    }

    private static bool Matches(string? pattern, string? value)
    {
        if (string.IsNullOrWhiteSpace(pattern) || string.Equals(pattern, "*", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!pattern.Contains('*'))
        {
            return string.Equals(pattern, value, StringComparison.OrdinalIgnoreCase);
        }

        var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
        return Regex.IsMatch(value, regexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static bool MeetsSeverity(IEnumerable<string> minimumSeverities, string actualSeverity)
    {
        var actualIndex = Array.FindIndex(SeverityOrder, x => string.Equals(x, actualSeverity, StringComparison.OrdinalIgnoreCase));
        if (actualIndex < 0)
        {
            actualIndex = Array.FindIndex(SeverityOrder, x => string.Equals(x, "Information", StringComparison.OrdinalIgnoreCase));
        }

        return minimumSeverities.Any(min =>
        {
            var minIndex = Array.FindIndex(SeverityOrder, x => string.Equals(x, min, StringComparison.OrdinalIgnoreCase));
            return minIndex >= 0 && actualIndex >= minIndex;
        });
    }

    private static SystemLog MapSystemLog(OperationalEvent operationalEvent)
    {
        return new SystemLog
        {
            Timestamp = DateTimeOffset.UtcNow,
            TimeZone = TimeZoneInfo.Local.Id,
            Level = operationalEvent.Severity,
            Category = operationalEvent.Category,
            Source = operationalEvent.Source,
            Message = operationalEvent.Message,
            Exception = operationalEvent.ExceptionMessage,
            StackTrace = operationalEvent.StackTrace,
            UserId = operationalEvent.UserId,
            Username = operationalEvent.Username,
            IpAddress = operationalEvent.IpAddress,
            UserAgent = operationalEvent.UserAgent,
            CorrelationId = operationalEvent.CorrelationId,
            HttpMethod = operationalEvent.HttpMethod,
            HttpPath = operationalEvent.HttpPath,
            HttpStatusCode = operationalEvent.HttpStatusCode,
            HttpDurationMs = operationalEvent.DurationMs,
            OperationName = operationalEvent.OperationName,
            DurationMs = operationalEvent.DurationMs,
            MachineName = Environment.MachineName,
            ProcessId = Environment.ProcessId,
            ThreadId = Environment.CurrentManagedThreadId,
            Properties = operationalEvent.Properties.Count == 0 ? null : JsonSerializer.Serialize(operationalEvent.Properties)
        };
    }

    private static SecurityEventLog MapSecurityLog(OperationalEvent operationalEvent)
    {
        return new SecurityEventLog
        {
            Timestamp = DateTimeOffset.UtcNow,
            EventType = operationalEvent.EventName,
            Severity = operationalEvent.Severity,
            UserId = operationalEvent.UserId,
            Username = operationalEvent.Username,
            IpAddress = operationalEvent.IpAddress,
            UserAgent = operationalEvent.UserAgent,
            Resource = operationalEvent.Resource ?? operationalEvent.HttpPath,
            Action = operationalEvent.Action ?? operationalEvent.OperationName,
            IsSuccessful = operationalEvent.IsSuccessful,
            FailureReason = operationalEvent.FailureReason ?? operationalEvent.ExceptionMessage,
            AdditionalData = operationalEvent.Properties.Count == 0 ? null : JsonSerializer.Serialize(operationalEvent.Properties)
        };
    }

    private static PerformanceLog MapPerformanceLog(OperationalEvent operationalEvent, int performanceThresholdMs)
    {
        return new PerformanceLog
        {
            Timestamp = DateTimeOffset.UtcNow,
            CorrelationId = operationalEvent.CorrelationId,
            UserId = operationalEvent.UserId,
            OperationName = operationalEvent.OperationName ?? operationalEvent.EventName,
            OperationType = operationalEvent.Category,
            DurationMs = operationalEvent.DurationMs ?? 0,
            MemoryBefore = 0,
            MemoryAfter = GC.GetTotalMemory(false),
            MemoryUsed = GC.GetTotalMemory(false),
            IsSlowOperation = (operationalEvent.DurationMs ?? 0) >= performanceThresholdMs,
            ThresholdMs = performanceThresholdMs,
            AdditionalData = operationalEvent.Properties.Count == 0 ? null : JsonSerializer.Serialize(operationalEvent.Properties)
        };
    }

    private static NotificationMessage MapNotification(OperationalEvent operationalEvent)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Event: {operationalEvent.EventName}");
        builder.AppendLine($"Severity: {operationalEvent.Severity}");
        builder.AppendLine($"Category: {operationalEvent.Category}");
        builder.AppendLine($"Message: {operationalEvent.Message}");

        if (!string.IsNullOrWhiteSpace(operationalEvent.OperationName))
        {
            builder.AppendLine($"Operation: {operationalEvent.OperationName}");
        }

        if (!string.IsNullOrWhiteSpace(operationalEvent.HttpPath))
        {
            builder.AppendLine($"Path: {operationalEvent.HttpPath}");
        }

        if (!string.IsNullOrWhiteSpace(operationalEvent.FailureReason))
        {
            builder.AppendLine($"Failure: {operationalEvent.FailureReason}");
        }

        if (operationalEvent.Properties.Count > 0)
        {
            builder.AppendLine("Properties:");
            builder.AppendLine(JsonSerializer.Serialize(operationalEvent.Properties));
        }

        // Notification konusu logdan farklidir.
        // Burada amac tum payload'i dump etmek degil, olayi hizli anlasilir hale getirmektir.
        return new NotificationMessage
        {
            EventName = operationalEvent.EventName,
            Severity = operationalEvent.Severity,
            Subject = $"[{operationalEvent.Severity}] {operationalEvent.EventName}",
            Body = builder.ToString(),
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["category"] = operationalEvent.Category,
                ["correlationId"] = operationalEvent.CorrelationId ?? string.Empty
            }
        };
    }
}
