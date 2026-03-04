using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;

namespace Host.Api.Integrations.Services;

public sealed class EmailDeliveryService(LogDbContext logDbContext) : IEmailDeliveryService
{
    public async Task SendAsync(string to, string subject, string body, string? attachmentPath, CancellationToken cancellationToken)
    {
        var systemLog = new SystemLog
        {
            Timestamp = DateTimeOffset.UtcNow,
            TimeZone = TimeZoneInfo.Local.Id,
            Level = "Information",
            Category = "OutboxMail",
            Source = nameof(EmailDeliveryService),
            Message = $"Mail dispatch simulated. To={to}, Subject={subject}",
            MessageTemplate = "Mail dispatch simulated. To={To}, Subject={Subject}",
            HttpStatusCode = StatusCodes.Status202Accepted,
            Exception = attachmentPath is null ? null : $"Attachment={attachmentPath}",
            MachineName = Environment.MachineName,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            ApplicationName = "Host.Api",
            ApplicationVersion = typeof(EmailDeliveryService).Assembly.GetName().Version?.ToString(),
            ProcessId = Environment.ProcessId,
            ThreadId = Environment.CurrentManagedThreadId
        };

        logDbContext.SystemLogs.Add(systemLog);
        await logDbContext.SaveChangesAsync(cancellationToken);
    }
}
