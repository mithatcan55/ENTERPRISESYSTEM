using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Operations.Application.Contracts;
using Operations.Application.Services;
using System.Text;

namespace Operations.Infrastructure.Services;

public sealed class OperationsLogQueryService(
    LogDbContext logDbContext,
    IdentityDbContext identityDbContext) : IOperationsLogQueryService
{
    public async Task<PagedResult<SystemLogListItemDto>> QuerySystemLogsAsync(LogQueryRequest request, CancellationToken cancellationToken)
    {
        // Tum log sorgularinda ortak davranis ayni:
        // filtreleri uygula, sayfalama normalizasyonu yap, DTO projection ile disari cikar.
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

    public async Task<PagedResult<EntityChangeLogListItemDto>> QueryEntityChangeLogsAsync(LogQueryRequest request, CancellationToken cancellationToken)
    {
        var (page, pageSize) = NormalizePaging(request.Page, request.PageSize);

        var query = logDbContext.EntityChangeLogs.AsNoTracking().AsQueryable();

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
                (x.EntityType ?? string.Empty).ToLower().Contains(search)
                || (x.EntityId ?? string.Empty).ToLower().Contains(search)
                || (x.Action ?? string.Empty).ToLower().Contains(search)
                || (x.TableName ?? string.Empty).ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new EntityChangeLogListItemDto(
                x.Id,
                x.Timestamp,
                x.CorrelationId,
                x.UserId,
                x.Username,
                x.EntityType,
                x.EntityId,
                x.Action,
                x.TableName,
                x.SchemaName,
                x.ChangedProperties))
            .ToListAsync(cancellationToken);

        return new PagedResult<EntityChangeLogListItemDto>(items, page, pageSize, totalCount);
    }

    public async Task<string> ExportEntityChangeLogsCsvAsync(LogQueryRequest request, CancellationToken cancellationToken)
    {
        // Export icin sinirli bir batch aliyoruz.
        // Bu ilk surumde operasyonel kullanim icin yeterli; buyudukce streaming export dusunulebilir.
        var exportRequest = new LogQueryRequest
        {
            Page = 1,
            PageSize = 1000,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            CorrelationId = request.CorrelationId,
            Search = request.Search
        };

        var result = await QueryEntityChangeLogsAsync(exportRequest, cancellationToken);

        var csv = new StringBuilder();
        csv.AppendLine("Id,Timestamp,CorrelationId,UserId,Username,EntityType,EntityId,Action,TableName,SchemaName,ChangedProperties");

        foreach (var item in result.Items)
        {
            csv.AppendLine(string.Join(",",
                EscapeCsv(item.Id.ToString()),
                EscapeCsv(item.Timestamp.ToString("O")),
                EscapeCsv(item.CorrelationId),
                EscapeCsv(item.UserId),
                EscapeCsv(item.Username),
                EscapeCsv(item.EntityType),
                EscapeCsv(item.EntityId),
                EscapeCsv(item.Action),
                EscapeCsv(item.TableName),
                EscapeCsv(item.SchemaName),
                EscapeCsv(item.ChangedProperties)));
        }

        return csv.ToString();
    }

    public async Task<PagedResult<SessionAdminListItemDto>> QuerySessionsAdminAsync(SessionAdminQueryRequest request, CancellationToken cancellationToken)
    {
        var (page, pageSize) = NormalizePaging(request.Page, request.PageSize);

        // Session loglari sadece log db'den degil, identity tarafindaki aktif oturum verilerinden de beslenir.
        var query =
            from session in identityDbContext.UserSessions.AsNoTracking()
            join user in identityDbContext.Users.AsNoTracking() on session.UserId equals user.Id
            where !session.IsDeleted && !user.IsDeleted
            select new { session, user };

        if (request.UserId.HasValue)
        {
            query = query.Where(x => x.session.UserId == request.UserId.Value);
        }

        if (!request.IncludeRevoked)
        {
            query = query.Where(x => !x.session.IsRevoked);
        }

        if (request.StartAt.HasValue)
        {
            query = query.Where(x => x.session.StartedAt >= request.StartAt.Value);
        }

        if (request.EndAt.HasValue)
        {
            query = query.Where(x => x.session.StartedAt <= request.EndAt.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                (x.user.UserCode ?? string.Empty).ToLower().Contains(search)
                || (x.user.UserCode ?? string.Empty).ToLower().Contains(search)
                || (x.user.Email ?? string.Empty).ToLower().Contains(search)
                || (x.session.SessionKey ?? string.Empty).ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.session.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SessionAdminListItemDto(
                x.session.Id,
                x.session.UserId,
                x.session.SessionKey,
                x.session.StartedAt,
                x.session.ExpiresAt,
                x.session.LastSeenAt,
                x.session.IsRevoked,
                x.session.RevokedAt,
                x.session.RevokedBy,
                x.session.ClientIpAddress,
                x.session.UserAgent,
                x.user.UserCode,
                x.user.UserCode,
                x.user.Email,
                x.user.IsActive))
            .ToListAsync(cancellationToken);

        return new PagedResult<SessionAdminListItemDto>(items, page, pageSize, totalCount);
    }

    private static (int Page, int PageSize) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 200);
        return (normalizedPage, normalizedPageSize);
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
}
