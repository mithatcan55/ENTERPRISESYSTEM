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

        var effectiveEvent = ApplyEscalationRules(operationalEvent);

        if (route.WriteSystemLog)
        {
            await logEventWriter.WriteSystemAsync(MapSystemLog(effectiveEvent), cancellationToken);
        }

        if (route.WriteSecurityLog)
        {
            await logEventWriter.WriteSecurityAsync(MapSecurityLog(effectiveEvent), cancellationToken);
        }

        var performanceThresholdMs = route.PerformanceThresholdMs ?? routingOptions.Value.DefaultPerformanceThresholdMs;

        if (route.WritePerformanceLog && effectiveEvent.DurationMs.HasValue)
        {
            await logEventWriter.WritePerformanceAsync(MapPerformanceLog(effectiveEvent, performanceThresholdMs), cancellationToken);
        }

        var notificationChannels = ResolveNotificationChannels(route, effectiveEvent);

        if (notificationChannels.Count > 0 && !routingOptions.Value.DisableAllNotifications)
        {
            var notification = MapNotification(effectiveEvent);
            foreach (var channelName in notificationChannels)
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

            return ApplyPreset(route);
        }

        return null;
    }

    private OperationalEvent ApplyEscalationRules(OperationalEvent operationalEvent)
    {
        var escalated = CloneEvent(operationalEvent);

        foreach (var rule in routingOptions.Value.EscalationRules)
        {
            if (!MatchesCriteria(rule, escalated))
            {
                continue;
            }

            if (rule.MinimumSeverities.Count > 0 && !MeetsSeverity(rule.MinimumSeverities, escalated.Severity))
            {
                continue;
            }

            if (rule.MinimumDurationMs.HasValue && (!escalated.DurationMs.HasValue || escalated.DurationMs.Value < rule.MinimumDurationMs.Value))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(rule.OverrideSeverity))
            {
                escalated.Severity = rule.OverrideSeverity;
            }

            if (rule.ForceNotification)
            {
                escalated.Properties["escalationChannels"] = rule.NotificationChannels;
                escalated.Properties["escalated"] = true;
            }
        }

        return escalated;
    }

    private static OperationalEvent CloneEvent(OperationalEvent source)
    {
        return new OperationalEvent
        {
            EventName = source.EventName,
            Category = source.Category,
            Severity = source.Severity,
            Message = source.Message,
            Source = source.Source,
            OperationName = source.OperationName,
            IsSuccessful = source.IsSuccessful,
            FailureReason = source.FailureReason,
            CorrelationId = source.CorrelationId,
            UserId = source.UserId,
            Username = source.Username,
            IpAddress = source.IpAddress,
            UserAgent = source.UserAgent,
            HttpMethod = source.HttpMethod,
            HttpPath = source.HttpPath,
            HttpStatusCode = source.HttpStatusCode,
            DurationMs = source.DurationMs,
            Resource = source.Resource,
            Action = source.Action,
            ExceptionMessage = source.ExceptionMessage,
            StackTrace = source.StackTrace,
            Properties = new Dictionary<string, object?>(source.Properties, StringComparer.OrdinalIgnoreCase)
        };
    }

    private List<string> ResolveNotificationChannels(OperationalEventRouteOptions route, OperationalEvent effectiveEvent)
    {
        var channels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (route.SendNotification)
        {
            var routeChannels = route.NotificationChannels.Count == 0
                ? _channels.Keys
                : route.NotificationChannels;

            foreach (var channel in routeChannels)
            {
                channels.Add(channel);
            }
        }

        if (effectiveEvent.Properties.TryGetValue("escalationChannels", out var escalationChannelsObj)
            && escalationChannelsObj is IEnumerable<string> escalationChannels)
        {
            foreach (var channel in escalationChannels)
            {
                channels.Add(channel);
            }
        }

        return channels.ToList();
    }

    private OperationalEventRouteOptions ApplyPreset(OperationalEventRouteOptions route)
    {
        if (string.IsNullOrWhiteSpace(route.Preset))
        {
            return route;
        }

        var preset = routingOptions.Value.Presets
            .FirstOrDefault(x => string.Equals(x.Name, route.Preset, StringComparison.OrdinalIgnoreCase));

        if (preset is null)
        {
            return route;
        }

        return new OperationalEventRouteOptions
        {
            EventName = route.EventName,
            Category = route.Category,
            Source = route.Source,
            OperationName = route.OperationName,
            OnlyFailures = route.OnlyFailures,
            OnlySuccess = route.OnlySuccess,
            MinimumHttpStatusCode = route.MinimumHttpStatusCode,
            MaximumHttpStatusCode = route.MaximumHttpStatusCode,
            Preset = route.Preset,
            WriteSystemLog = route.WriteSystemLog || preset.WriteSystemLog,
            WriteSecurityLog = route.WriteSecurityLog || preset.WriteSecurityLog,
            WritePerformanceLog = route.WritePerformanceLog || preset.WritePerformanceLog,
            PerformanceThresholdMs = route.PerformanceThresholdMs ?? preset.PerformanceThresholdMs,
            SendNotification = route.SendNotification || preset.SendNotification,
            NotificationChannels = route.NotificationChannels.Count == 0 ? preset.NotificationChannels : route.NotificationChannels,
            MinimumSeverities = route.MinimumSeverities.Count == 0 ? preset.MinimumSeverities : route.MinimumSeverities
        };
    }

    private static bool MatchesCriteria(OperationalEventRouteCriteria criteria, OperationalEvent operationalEvent)
    {
        if (!Matches(criteria.EventName, operationalEvent.EventName)
            || !Matches(criteria.Category, operationalEvent.Category)
            || !Matches(criteria.Source, operationalEvent.Source)
            || !Matches(criteria.OperationName, operationalEvent.OperationName))
        {
            return false;
        }

        if (criteria.OnlyFailures && operationalEvent.IsSuccessful != false)
        {
            return false;
        }

        if (criteria.OnlySuccess && operationalEvent.IsSuccessful != true)
        {
            return false;
        }

        if (criteria.MinimumHttpStatusCode.HasValue && (!operationalEvent.HttpStatusCode.HasValue || operationalEvent.HttpStatusCode.Value < criteria.MinimumHttpStatusCode.Value))
        {
            return false;
        }

        if (criteria.MaximumHttpStatusCode.HasValue && (!operationalEvent.HttpStatusCode.HasValue || operationalEvent.HttpStatusCode.Value > criteria.MaximumHttpStatusCode.Value))
        {
            return false;
        }

        return true;
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
