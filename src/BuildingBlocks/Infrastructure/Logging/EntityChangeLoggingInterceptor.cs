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
    private sealed class PendingEntityChange
    {
        public required EntityEntry Entry { get; init; }
        public required EntityState State { get; init; }
        public required string TableName { get; init; }
        public string? SchemaName { get; init; }
        public required List<string> ChangedProperties { get; init; }
        public Dictionary<string, object?>? OldValues { get; init; }
        public Dictionary<string, object?>? NewValues { get; init; }
    }

    private readonly ConcurrentDictionary<Guid, List<PendingEntityChange>> _pendingChanges = new();

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
            .Select(CreatePendingChange)
            .Where(x => x is not null)
            .Cast<PendingEntityChange>()
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

        var logs = changes
            .Select(CreateEntityChangeLog)
            .ToList();

        if (logs.Count == 0)
        {
            return;
        }

        await logEventWriter.WriteEntityChangesAsync(logs, cancellationToken);
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

    private PendingEntityChange? CreatePendingChange(EntityEntry entry)
    {
        var tableName = entry.Metadata.GetTableName();
        if (string.IsNullOrWhiteSpace(tableName))
        {
            return null;
        }

        var changedProperties = entry.Properties
            .Where(x => ShouldIncludeProperty(entry, x))
            .Select(x => x.Metadata.Name)
            .Distinct()
            .ToList();

        if (changedProperties.Count == 0)
        {
            return null;
        }

        return new PendingEntityChange
        {
            Entry = entry,
            State = entry.State,
            TableName = tableName,
            SchemaName = entry.Metadata.GetSchema(),
            ChangedProperties = changedProperties,
            OldValues = BuildOldValues(entry, changedProperties),
            NewValues = BuildNewValues(entry, changedProperties)
        };
    }

    private EntityChangeLog CreateEntityChangeLog(PendingEntityChange pending)
    {
        return new EntityChangeLog
        {
            Timestamp = DateTimeOffset.UtcNow,
            CorrelationId = Activity.Current?.Id,
            UserId = SafeGetActorId(),
            EntityType = pending.Entry.Metadata.ClrType.Name,
            EntityId = ResolveEntityId(pending.Entry),
            Action = pending.State.ToString().ToUpperInvariant(),
            OldValues = SerializeValues(pending.OldValues),
            NewValues = SerializeValues(pending.NewValues),
            ChangedProperties = JsonSerializer.Serialize(pending.ChangedProperties),
            TableName = pending.TableName,
            SchemaName = pending.SchemaName
        };
    }

    private static bool ShouldIncludeProperty(EntityEntry entry, PropertyEntry property)
    {
        if (property.Metadata.Name is "CreatedAt"
            or "CreatedBy"
            or "ModifiedAt"
            or "ModifiedBy"
            or "DeletedAt"
            or "DeletedBy")
        {
            return false;
        }

        if (property.Metadata.IsPrimaryKey())
        {
            return true;
        }

        return entry.State switch
        {
            EntityState.Added => true,
            EntityState.Deleted => true,
            EntityState.Modified => property.IsModified,
            _ => false
        };
    }

    private static Dictionary<string, object?>? BuildOldValues(EntityEntry entry, List<string> changedProperties)
    {
        if (entry.State == EntityState.Added)
        {
            return null;
        }

        return changedProperties.ToDictionary(
            name => name,
            name => entry.Property(name).OriginalValue);
    }

    private static Dictionary<string, object?>? BuildNewValues(EntityEntry entry, List<string> changedProperties)
    {
        if (entry.State == EntityState.Deleted)
        {
            return null;
        }

        return changedProperties.ToDictionary(
            name => name,
            name => entry.Property(name).CurrentValue);
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

    private static string? SerializeValues(Dictionary<string, object?>? values)
        => values is null || values.Count == 0 ? null : JsonSerializer.Serialize(values);

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
