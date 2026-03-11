using Integrations.Application.Contracts;

namespace Integrations.Application.Services;

public interface IExternalOutboxService
{
    Task<OutboxMessageQueuedDto> QueueMailAsync(QueueMailRequest request, CancellationToken cancellationToken);
    Task<OutboxMessageQueuedDto> QueueExcelReportAsync(QueueExcelReportRequest request, CancellationToken cancellationToken);
    Task<OutboxPagedResult<OutboxMessageListItemDto>> ListMessagesAsync(OutboxMessageQueryRequest request, CancellationToken cancellationToken);
}
