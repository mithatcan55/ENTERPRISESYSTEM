using Host.Api.Integrations.Contracts;

namespace Host.Api.Integrations.Services;

public interface IExternalOutboxService
{
    Task<OutboxMessageQueuedDto> QueueMailAsync(QueueMailRequest request, CancellationToken cancellationToken);
    Task<OutboxMessageQueuedDto> QueueExcelReportAsync(QueueExcelReportRequest request, CancellationToken cancellationToken);
}
