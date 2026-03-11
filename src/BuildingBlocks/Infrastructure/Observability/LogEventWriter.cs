using System.Diagnostics;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Observability;

public sealed class LogEventWriter(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<LogEventWriter> logger) : ILogEventWriter
{
    public Task WriteDatabaseQueryAsync(DatabaseQueryLog log, CancellationToken cancellationToken = default)
        => WriteAsync(db => db.DatabaseQueryLogs.Add(log), cancellationToken);

    public Task WriteSystemAsync(SystemLog log, CancellationToken cancellationToken = default)
        => WriteAsync(db => db.SystemLogs.Add(log), cancellationToken);

    public Task WriteSecurityAsync(SecurityEventLog log, CancellationToken cancellationToken = default)
        => WriteAsync(db => db.SecurityEventLogs.Add(log), cancellationToken);

    public Task WriteHttpAsync(HttpRequestLog log, CancellationToken cancellationToken = default)
        => WriteAsync(db => db.HttpRequestLogs.Add(log), cancellationToken);

    public Task WritePerformanceAsync(PerformanceLog log, CancellationToken cancellationToken = default)
        => WriteAsync(db => db.PerformanceLogs.Add(log), cancellationToken);

    public Task WritePageVisitAsync(PageVisitLog log, CancellationToken cancellationToken = default)
        => WriteAsync(db => db.PageVisitLogs.Add(log), cancellationToken);

    public Task WriteEntityChangesAsync(IEnumerable<EntityChangeLog> logs, CancellationToken cancellationToken = default)
        => WriteAsync(db => db.EntityChangeLogs.AddRange(logs), cancellationToken);

    private async Task WriteAsync(Action<LogDbContext> writeAction, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var logDbContext = scope.ServiceProvider.GetRequiredService<LogDbContext>();
            writeAction(logDbContext);
            await logDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Structured log persistence failed. TraceId={TraceId}", Activity.Current?.Id);
        }
    }
}
