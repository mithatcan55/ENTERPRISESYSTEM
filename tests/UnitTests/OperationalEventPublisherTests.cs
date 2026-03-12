using Application.Observability;
using Infrastructure.Observability;
using Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Options;

namespace UnitTests;

public sealed class OperationalEventPublisherTests
{
    [Fact]
    public async Task Publish_Should_Apply_Preset_And_Escalation_Rules_Before_Routing()
    {
        var writer = new CapturingLogEventWriter();
        var emailChannel = new CapturingNotificationChannel("email");
        var webhookChannel = new CapturingNotificationChannel("webhook");

        var options = Options.Create(new ObservabilityRoutingOptions
        {
            DefaultPerformanceThresholdMs = 500,
            Presets =
            [
                new OperationalEventRoutePresetOptions
                {
                    Name = "operation-trace",
                    WriteSystemLog = true,
                    WritePerformanceLog = true,
                    MinimumSeverities = ["Information"]
                }
            ],
            Routes =
            [
                new OperationalEventRouteOptions
                {
                    EventName = "BusinessOperationFailed",
                    Preset = "operation-trace",
                    OnlyFailures = true,
                    MinimumSeverities = ["Warning"]
                }
            ],
            EscalationRules =
            [
                new OperationalEventEscalationRuleOptions
                {
                    EventName = "BusinessOperationFailed",
                    MinimumDurationMs = 2000,
                    MinimumSeverities = ["Warning"],
                    ForceNotification = true,
                    NotificationChannels = ["email"],
                    OverrideSeverity = "Error"
                }
            ]
        });

        var publisher = new OperationalEventPublisher(writer, [emailChannel, webhookChannel], options);

        await publisher.PublishAsync(new OperationalEvent
        {
            EventName = "BusinessOperationFailed",
            Category = "Operation",
            Severity = "Warning",
            Message = "CreateUser failed",
            OperationName = "CreateUser",
            IsSuccessful = false,
            DurationMs = 2500
        });

        var systemLog = Assert.Single(writer.SystemLogs);
        Assert.Equal("Error", systemLog.Level);

        var performanceLog = Assert.Single(writer.PerformanceLogs);
        Assert.Equal(2500, performanceLog.DurationMs);
        Assert.True(performanceLog.IsSlowOperation);

        var emailNotification = Assert.Single(emailChannel.Messages);
        Assert.Contains("BusinessOperationFailed", emailNotification.Subject);
        Assert.Empty(webhookChannel.Messages);
    }

    [Fact]
    public async Task Publish_Should_Match_Wildcard_Category_Routes()
    {
        var writer = new CapturingLogEventWriter();
        var webhookChannel = new CapturingNotificationChannel("webhook");

        var options = Options.Create(new ObservabilityRoutingOptions
        {
            Routes =
            [
                new OperationalEventRouteOptions
                {
                    EventName = "*",
                    Category = "Auth*",
                    WriteSystemLog = true,
                    WriteSecurityLog = true,
                    SendNotification = true,
                    NotificationChannels = ["webhook"],
                    OnlyFailures = true,
                    MinimumSeverities = ["Warning"]
                }
            ]
        });

        var publisher = new OperationalEventPublisher(writer, [webhookChannel], options);

        await publisher.PublishAsync(new OperationalEvent
        {
            EventName = "AuthLifecycleFailed",
            Category = "Authentication",
            Severity = "Warning",
            Message = "Login failed",
            IsSuccessful = false
        });

        Assert.Single(writer.SystemLogs);
        Assert.Single(writer.SecurityLogs);
        Assert.Single(webhookChannel.Messages);
    }

    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public List<SystemLog> SystemLogs { get; } = [];
        public List<SecurityEventLog> SecurityLogs { get; } = [];
        public List<PerformanceLog> PerformanceLogs { get; } = [];

        public Task WriteDatabaseQueryAsync(DatabaseQueryLog log, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task WriteHttpAsync(HttpRequestLog log, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task WritePageVisitAsync(PageVisitLog log, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task WriteEntityChangesAsync(IEnumerable<EntityChangeLog> logs, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task WriteSystemAsync(SystemLog log, CancellationToken cancellationToken = default)
        {
            SystemLogs.Add(log);
            return Task.CompletedTask;
        }

        public Task WriteSecurityAsync(SecurityEventLog log, CancellationToken cancellationToken = default)
        {
            SecurityLogs.Add(log);
            return Task.CompletedTask;
        }

        public Task WritePerformanceAsync(PerformanceLog log, CancellationToken cancellationToken = default)
        {
            PerformanceLogs.Add(log);
            return Task.CompletedTask;
        }
    }

    private sealed class CapturingNotificationChannel(string channelName) : INotificationChannel
    {
        public string ChannelName => channelName;
        public List<NotificationMessage> Messages { get; } = [];

        public Task PublishAsync(NotificationMessage notification, CancellationToken cancellationToken = default)
        {
            Messages.Add(notification);
            return Task.CompletedTask;
        }
    }
}
