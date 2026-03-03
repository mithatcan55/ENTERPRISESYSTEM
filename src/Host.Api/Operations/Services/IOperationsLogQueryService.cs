using Host.Api.Operations.Contracts;

namespace Host.Api.Operations.Services;

public interface IOperationsLogQueryService
{
    Task<PagedResult<SystemLogListItemDto>> QuerySystemLogsAsync(LogQueryRequest request, CancellationToken cancellationToken);
    Task<PagedResult<SecurityEventListItemDto>> QuerySecurityEventsAsync(LogQueryRequest request, CancellationToken cancellationToken);
    Task<PagedResult<HttpRequestLogListItemDto>> QueryHttpRequestLogsAsync(LogQueryRequest request, CancellationToken cancellationToken);
}
