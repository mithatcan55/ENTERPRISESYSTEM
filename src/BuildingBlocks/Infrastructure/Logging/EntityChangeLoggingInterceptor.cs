using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Infrastructure.Observability;
using Infrastructure.Persistence.Auditing;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Logging;

public sealed class EntityChangeLoggingInterceptor(
    ILogEventWriter logEventWriter,
    IAuditActorAccessor auditActorAccessor) : SaveChangesInterceptor
{
    private readonly ConcurrentDictionary<Guid, List<EntityChangeLog>> _pendingChanges = new();

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        CaptureChanges(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CaptureChanges(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        FlushChangesAsync(eventData.Context, CancellationToken.None).GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await FlushChangesAsync(eventData.Context, cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        if (eventData.Context is not null)
        {
            _pendingChanges.TryRemove(eventData.Context.ContextId.InstanceId, out _);
        }

        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            _pendingChanges.TryRemove(eventData.Context.ContextId.InstanceId, out _);
        }

        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private void CaptureChanges(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var changes = dbContext.ChangeTracker.Entries()
            .Where(ShouldLog)
            .Select(CreateChangeLog)
            .Where(x => x is not null)
            .Cast<EntityChangeLog>()
            .ToList();

        if (changes.Count == 0)
        {
            return;
        }

        _pendingChanges[dbContext.ContextId.InstanceId] = changes;
    }

    private async Task FlushChangesAsync(DbContext? dbContext, CancellationToken cancellationToken)
    {
        if (dbContext is null)
        {
            return;
        }

        if (!_pendingChanges.TryRemove(dbContext.ContextId.InstanceId, out var changes) || changes.Count == 0)
        {
            return;
        }

        await logEventWriter.WriteEntityChangesAsync(changes, cancellationToken);
    }

    private bool ShouldLog(EntityEntry entry)
    {
        if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            return false;
        }

        var entityType = entry.Metadata.ClrType;
        return entityType != typeof(DatabaseQueryLog)
               && entityType != typeof(EntityChangeLog)
               && entityType != typeof(HttpRequestLog)
               && entityType != typeof(PageVisitLog)
               && entityType != typeof(PerformanceLog)
               && entityType != typeof(SecurityEventLog)
               && entityType != typeof(SystemLog);
    }

    private EntityChangeLog? CreateChangeLog(EntityEntry entry)
    {
        var tableName = entry.Metadata.GetTableName();
        if (string.IsNullOrWhiteSpace(tableName))
        {
            return null;
        }

        var changedProperties = entry.Properties
            .Where(ShouldIncludeProperty)
            .Select(x => x.Metadata.Name)
            .Distinct()
            .ToList();

        var oldValues = entry.State == EntityState.Added
            ? null
            : SerializeValues(entry.Properties.Where(ShouldIncludeProperty).ToDictionary(x => x.Metadata.Name, x => entry.State == EntityState.Deleted ? x.OriginalValue : x.OriginalValue));

        var newValues = entry.State == EntityState.Deleted
            ? null
            : SerializeValues(entry.Properties.Where(ShouldIncludeProperty).ToDictionary(x => x.Metadata.Name, x => x.CurrentValue));

        return new EntityChangeLog
        {
            Timestamp = DateTimeOffset.UtcNow,
            CorrelationId = Activity.Current?.Id,
            UserId = SafeGetActorId(),
            EntityType = entry.Metadata.ClrType.Name,
            EntityId = ResolveEntityId(entry),
            Action = entry.State.ToString().ToUpperInvariant(),
            OldValues = oldValues,
            NewValues = newValues,
            ChangedProperties = JsonSerializer.Serialize(changedProperties),
            TableName = tableName,
            SchemaName = entry.Metadata.GetSchema()
        };
    }

    private static bool ShouldIncludeProperty(PropertyEntry property)
    {
        if (property.Metadata.IsPrimaryKey())
        {
            return true;
        }

        return property.Metadata.Name is not "CreatedAt"
            and not "CreatedBy"
            and not "ModifiedAt"
            and not "ModifiedBy"
            and not "DeletedAt"
            and not "DeletedBy";
    }

    private static string? ResolveEntityId(EntityEntry entry)
    {
        var primaryKey = entry.Properties.FirstOrDefault(x => x.Metadata.IsPrimaryKey());
        if (primaryKey is null || primaryKey.IsTemporary)
        {
            return null;
        }

        return primaryKey.CurrentValue?.ToString() ?? primaryKey.OriginalValue?.ToString();
    }

    private static string? SerializeValues(Dictionary<string, object?> values)
        => values.Count == 0 ? null : JsonSerializer.Serialize(values);

    private string SafeGetActorId()
    {
        try
        {
            return auditActorAccessor.GetActorId();
        }
        catch
        {
            return "system";
        }
    }
}
