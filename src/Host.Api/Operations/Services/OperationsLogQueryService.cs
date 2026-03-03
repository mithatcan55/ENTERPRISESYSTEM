using Host.Api.Operations.Contracts;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Host.Api.Operations.Services;

public sealed class OperationsLogQueryService(LogDbContext logDbContext) : IOperationsLogQueryService
{
    public async Task<PagedResult<SystemLogListItemDto>> QuerySystemLogsAsync(LogQueryRequest request, CancellationToken cancellationToken)
    {
        var (page, pageSize) = NormalizePaging(request.Page, request.PageSize);

        var query = logDbContext.SystemLogs.AsNoTracking().AsQueryable();

        if (request.StartAt.HasValue)
        {
            query = query.Where(x => x.Timestamp >= request.StartAt.Value);
        }

        if (request.EndAt.HasValue)
        {
            query = query.Where(x => x.Timestamp <= request.EndAt.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.CorrelationId))
        {
            var correlationId = request.CorrelationId.Trim();
            query = query.Where(x => x.CorrelationId == correlationId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                (x.Message ?? string.Empty).ToLower().Contains(search)
                || (x.Source ?? string.Empty).ToLower().Contains(search)
                || (x.Category ?? string.Empty).ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SystemLogListItemDto(
                x.Id,
                x.Timestamp,
                x.Level,
                x.Category,
                x.Source,
                x.Message,
                x.CorrelationId,
                x.HttpStatusCode,
                x.UserId,
                x.Username))
            .ToListAsync(cancellationToken);

        return new PagedResult<SystemLogListItemDto>(items, page, pageSize, totalCount);
    }

    public async Task<PagedResult<SecurityEventListItemDto>> QuerySecurityEventsAsync(LogQueryRequest request, CancellationToken cancellationToken)
    {
        var (page, pageSize) = NormalizePaging(request.Page, request.PageSize);

        var query = logDbContext.SecurityEventLogs.AsNoTracking().AsQueryable();

        if (request.StartAt.HasValue)
        {
            query = query.Where(x => x.Timestamp >= request.StartAt.Value);
        }

        if (request.EndAt.HasValue)
        {
            query = query.Where(x => x.Timestamp <= request.EndAt.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                (x.EventType ?? string.Empty).ToLower().Contains(search)
                || (x.Action ?? string.Empty).ToLower().Contains(search)
                || (x.Resource ?? string.Empty).ToLower().Contains(search)
                || (x.FailureReason ?? string.Empty).ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SecurityEventListItemDto(
                x.Id,
                x.Timestamp,
                x.EventType,
                x.Severity,
                x.UserId,
                x.Username,
                x.Resource,
                x.Action,
                x.IsSuccessful,
                x.FailureReason,
                x.IpAddress))
            .ToListAsync(cancellationToken);

        return new PagedResult<SecurityEventListItemDto>(items, page, pageSize, totalCount);
    }

    public async Task<PagedResult<HttpRequestLogListItemDto>> QueryHttpRequestLogsAsync(LogQueryRequest request, CancellationToken cancellationToken)
    {
        var (page, pageSize) = NormalizePaging(request.Page, request.PageSize);

        var query = logDbContext.HttpRequestLogs.AsNoTracking().AsQueryable();

        if (request.StartAt.HasValue)
        {
            query = query.Where(x => x.Timestamp >= request.StartAt.Value);
        }

        if (request.EndAt.HasValue)
        {
            query = query.Where(x => x.Timestamp <= request.EndAt.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.CorrelationId))
        {
            var correlationId = request.CorrelationId.Trim();
            query = query.Where(x => x.CorrelationId == correlationId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                (x.Method ?? string.Empty).ToLower().Contains(search)
                || (x.Path ?? string.Empty).ToLower().Contains(search)
                || (x.ErrorMessage ?? string.Empty).ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new HttpRequestLogListItemDto(
                x.Id,
                x.Timestamp,
                x.Method,
                x.Path,
                x.StatusCode,
                x.DurationMs,
                x.CorrelationId,
                x.IsError,
                x.UserId,
                x.Username,
                x.IpAddress))
            .ToListAsync(cancellationToken);

        return new PagedResult<HttpRequestLogListItemDto>(items, page, pageSize, totalCount);
    }

    private static (int Page, int PageSize) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 200);
        return (normalizedPage, normalizedPageSize);
    }
}
