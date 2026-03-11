using Infrastructure.Persistence.Entities;

namespace Infrastructure.Observability;

public interface ILogEventWriter
{
    Task WriteDatabaseQueryAsync(DatabaseQueryLog log, CancellationToken cancellationToken = default);
    Task WriteSystemAsync(SystemLog log, CancellationToken cancellationToken = default);
    Task WriteSecurityAsync(SecurityEventLog log, CancellationToken cancellationToken = default);
    Task WriteHttpAsync(HttpRequestLog log, CancellationToken cancellationToken = default);
    Task WritePerformanceAsync(PerformanceLog log, CancellationToken cancellationToken = default);
    Task WritePageVisitAsync(PageVisitLog log, CancellationToken cancellationToken = default);
    Task WriteEntityChangesAsync(IEnumerable<EntityChangeLog> logs, CancellationToken cancellationToken = default);
}
