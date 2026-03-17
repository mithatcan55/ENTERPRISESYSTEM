using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Operations.Application.Contracts;
using Operations.Application.Services;

namespace Operations.Infrastructure.Services;

public sealed class AuditDashboardService(
    LogDbContext logDbContext,
    IdentityDbContext identityDbContext) : IAuditDashboardService
{
    public async Task<AuditDashboardSummaryDto> GetSummaryAsync(int windowHours, CancellationToken cancellationToken)
    {
        // Dashboard ozeti tek bir metrikten degil, birden fazla kaynaktan beslenir.
        // Ancak log tablolari gecis halinde veya bos oldugunda dashboard'un 500 ile dusmesi istenmez.
        var normalizedHours = windowHours <= 0 ? 24 : Math.Min(windowHours, 24 * 14);
        var now = DateTimeOffset.UtcNow;

        try
        {
            var start = now.AddHours(-normalizedHours);

            var systemErrorCount = await logDbContext.SystemLogs
                .AsNoTracking()
                .CountAsync(x => x.Timestamp >= start && (x.Level == "Error" || x.HttpStatusCode >= 500), cancellationToken);

            var failedLoginQuery = logDbContext.SecurityEventLogs
                .AsNoTracking()
                .Where(x => x.Timestamp >= start
                            && x.Action == "Login"
                            && !x.IsSuccessful);

            var failedLoginCount = await failedLoginQuery.CountAsync(cancellationToken);

            var failedLoginRaw = await failedLoginQuery
                .Select(x => x.Timestamp)
                .ToListAsync(cancellationToken);

            var failedLoginTrend = failedLoginRaw
                .GroupBy(t => new DateTimeOffset(t.Year, t.Month, t.Day, t.Hour, 0, 0, TimeSpan.Zero))
                .OrderBy(g => g.Key)
                .Select(g => new HourlyMetricDto(g.Key, g.Count()))
                .ToList();

            var startedSessions = await identityDbContext.UserSessions
                .AsNoTracking()
                .CountAsync(x => !x.IsDeleted && x.StartedAt >= start.UtcDateTime, cancellationToken);

            var revokedSessions = await identityDbContext.UserSessions
                .AsNoTracking()
                .CountAsync(x => !x.IsDeleted && x.StartedAt >= start.UtcDateTime && x.IsRevoked, cancellationToken);

            var revokeRate = startedSessions == 0
                ? 0m
                : Math.Round((decimal)revokedSessions / startedSessions * 100m, 2);

            var topCriticalEvents = await logDbContext.SecurityEventLogs
                .AsNoTracking()
                .Where(x => x.Timestamp >= start
                            && (!x.IsSuccessful || x.Severity == "Warning" || x.Severity == "Error"))
                .GroupBy(x => x.EventType ?? "Unknown")
                .Select(g => new TopEventDto(g.Key, g.Count()))
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync(cancellationToken);

            return new AuditDashboardSummaryDto(
                GeneratedAt: now,
                WindowHours: normalizedHours,
                SystemErrorCount: systemErrorCount,
                FailedLoginCount: failedLoginCount,
                SessionRevokeRatePercent: revokeRate,
                FailedLoginTrend: failedLoginTrend,
                TopCriticalEvents: topCriticalEvents);
        }
        catch
        {
            return new AuditDashboardSummaryDto(
                GeneratedAt: now,
                WindowHours: normalizedHours,
                SystemErrorCount: 0,
                FailedLoginCount: 0,
                SessionRevokeRatePercent: 0,
                FailedLoginTrend: [],
                TopCriticalEvents: []);
        }
    }
}
