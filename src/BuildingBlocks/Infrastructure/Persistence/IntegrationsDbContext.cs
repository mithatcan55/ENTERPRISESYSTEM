using Infrastructure.Persistence.Auditing;
using Infrastructure.Persistence.Entities.Integration;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Auditing;

namespace Infrastructure.Persistence;

public sealed class IntegrationsDbContext(
    DbContextOptions<IntegrationsDbContext> options,
    IAuditActorAccessor auditActorAccessor) : DbContext(options)
{
    public DbSet<ExternalOutboxMessage> ExternalOutboxMessages => Set<ExternalOutboxMessage>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        AuditRulesApplicator.Apply(ChangeTracker, auditActorAccessor);
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        AuditRulesApplicator.Apply(ChangeTracker, auditActorAccessor);
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(PersistenceSchemaNames.Business);

        modelBuilder.Entity<ExternalOutboxMessage>(entity =>
        {
            entity.ToTable("ExternalOutboxMessages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("text").IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
            entity.Property(x => x.LastError).HasMaxLength(4000);
            entity.Property(x => x.CorrelationId).HasMaxLength(100);
            entity.Property(x => x.DeduplicationKey).HasMaxLength(250);
            entity.HasIndex(x => new { x.Status, x.NextAttemptAt });
            entity.HasIndex(x => x.DeduplicationKey);
        });
    }
}
