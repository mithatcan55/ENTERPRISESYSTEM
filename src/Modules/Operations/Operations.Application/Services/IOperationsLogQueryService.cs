using Operations.Application.Contracts;

namespace Operations.Application.Services;

public interface IOperationsLogQueryService
{
    Task<PagedResult<SystemLogListItemDto>> QuerySystemLogsAsync(LogQueryRequest request, CancellationToken cancellationToken);
    Task<PagedResult<SecurityEventListItemDto>> QuerySecurityEventsAsync(LogQueryRequest request, CancellationToken cancellationToken);
    Task<PagedResult<HttpRequestLogListItemDto>> QueryHttpRequestLogsAsync(LogQueryRequest request, CancellationToken cancellationToken);
    Task<PagedResult<EntityChangeLogListItemDto>> QueryEntityChangeLogsAsync(LogQueryRequest request, CancellationToken cancellationToken);
    Task<string> ExportEntityChangeLogsCsvAsync(LogQueryRequest request, CancellationToken cancellationToken);
    Task<PagedResult<SessionAdminListItemDto>> QuerySessionsAdminAsync(SessionAdminQueryRequest request, CancellationToken cancellationToken);
}
