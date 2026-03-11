using System.Text.Json;
using Application.Exceptions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Integration;
using Integrations.Application.Contracts;
using Integrations.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace Integrations.Infrastructure.Services;

public sealed class ExternalOutboxService(
    IntegrationsDbContext integrationsDbContext,
    IHttpContextAccessor httpContextAccessor) : IExternalOutboxService
{
    public async Task<OutboxPagedResult<OutboxMessageListItemDto>> ListMessagesAsync(OutboxMessageQueryRequest request, CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 50 : Math.Min(request.PageSize, 200);

        var query = integrationsDbContext.ExternalOutboxMessages
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.Trim();
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.EventType))
        {
            var eventType = request.EventType.Trim();
            query = query.Where(x => x.EventType == eventType);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                (x.LastError ?? string.Empty).ToLower().Contains(search)
                || (x.CorrelationId ?? string.Empty).ToLower().Contains(search)
                || (x.DeduplicationKey ?? string.Empty).ToLower().Contains(search)
                || (x.EventType ?? string.Empty).ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new OutboxMessageListItemDto(
                x.Id,
                x.CreatedAt,
                x.EventType,
                x.Status,
                x.AttemptCount,
                x.MaxAttempts,
                x.NextAttemptAt,
                x.ProcessedAt,
                x.LastError,
                x.CorrelationId,
                x.DeduplicationKey))
            .ToListAsync(cancellationToken);

        return new OutboxPagedResult<OutboxMessageListItemDto>(items, page, pageSize, totalCount);
    }

    public async Task<OutboxMessageQueuedDto> QueueMailAsync(QueueMailRequest request, CancellationToken cancellationToken)
    {
        // Outbox mantiginda dogrudan mail gondermiyoruz.
        // Once dayanıklı kuyruk kaydi olusturuyor, sonra dispatcher bu kaydi isliyor.
        if (string.IsNullOrWhiteSpace(request.To) || string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Body))
        {
            throw new ValidationAppException(
                "Mail kuyruk dogrulamasi basarisiz.",
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
        // Excel raporlarinda da ayni desen korunuyor:
        // istek alinir, payload serialize edilir, sonra dispatcher uygun kanala iletir.
        if (string.IsNullOrWhiteSpace(request.ReportName))
        {
            throw new ValidationAppException(
                "Excel kuyruk dogrulamasi basarisiz.",
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
        var correlationId = httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString()
                            ?? httpContextAccessor.HttpContext?.TraceIdentifier;

        // CorrelationId'yi outbox mesajina yazmak kritik.
        // Boylece asenkron islenen bir mesaji ilk HTTP istegiyle baglayabiliriz.
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

        integrationsDbContext.ExternalOutboxMessages.Add(message);
        await integrationsDbContext.SaveChangesAsync(cancellationToken);

        return new OutboxMessageQueuedDto(message.Id, message.EventType, message.Status, message.CreatedAt);
    }
}
