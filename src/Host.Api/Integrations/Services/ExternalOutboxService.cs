using System.Text.Json;
using Host.Api.Exceptions;
using Host.Api.Integrations.Contracts;
using Host.Api.Middleware;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Integration;

namespace Host.Api.Integrations.Services;

public sealed class ExternalOutboxService(
    BusinessDbContext businessDbContext,
    IHttpContextAccessor httpContextAccessor) : IExternalOutboxService
{
    public async Task<OutboxMessageQueuedDto> QueueMailAsync(QueueMailRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.To) || string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Body))
        {
            throw new ValidationAppException(
                "Mail kuyruk doğrulaması başarısız.",
                new Dictionary<string, string[]>
                {
                    ["request"] = ["to, subject ve body zorunludur."]
                });
        }

        var payload = new MailOutboxPayload
        {
            To = request.To.Trim(),
            Subject = request.Subject.Trim(),
            Body = request.Body.Trim()
        };

        return await EnqueueAsync(OutboxEventTypes.MailNotification, payload, cancellationToken);
    }

    public async Task<OutboxMessageQueuedDto> QueueExcelReportAsync(QueueExcelReportRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ReportName))
        {
            throw new ValidationAppException(
                "Excel kuyruk doğrulaması başarısız.",
                new Dictionary<string, string[]>
                {
                    ["reportName"] = ["reportName zorunludur."]
                });
        }

        var payload = new ExcelOutboxPayload
        {
            ReportName = request.ReportName.Trim(),
            Headers = request.Headers,
            Rows = request.Rows,
            NotifyEmail = request.NotifyEmail?.Trim()
        };

        return await EnqueueAsync(OutboxEventTypes.ExcelReport, payload, cancellationToken);
    }

    private async Task<OutboxMessageQueuedDto> EnqueueAsync<TPayload>(string eventType, TPayload payload, CancellationToken cancellationToken)
    {
        var correlationId = httpContextAccessor.HttpContext?.Items[CorrelationIdMiddleware.CorrelationItemKey]?.ToString()
                            ?? httpContextAccessor.HttpContext?.TraceIdentifier;

        var message = new ExternalOutboxMessage
        {
            EventType = eventType,
            PayloadJson = JsonSerializer.Serialize(payload),
            Status = "Pending",
            AttemptCount = 0,
            MaxAttempts = 5,
            NextAttemptAt = DateTime.UtcNow,
            CorrelationId = correlationId,
            DeduplicationKey = Guid.NewGuid().ToString("N")
        };

        businessDbContext.ExternalOutboxMessages.Add(message);
        await businessDbContext.SaveChangesAsync(cancellationToken);

        return new OutboxMessageQueuedDto(message.Id, message.EventType, message.Status, message.CreatedAt);
    }
}
