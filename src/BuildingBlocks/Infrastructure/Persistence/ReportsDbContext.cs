using Infrastructure.Persistence.Auditing;
using Infrastructure.Persistence.Entities.Reporting;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Auditing;

namespace Infrastructure.Persistence;

public sealed class ReportsDbContext(
    DbContextOptions<ReportsDbContext> options,
    IAuditActorAccessor auditActorAccessor) : DbContext(options)
{
    public DbSet<ReportTemplate> ReportTemplates => Set<ReportTemplate>();
    public DbSet<ReportTemplateVersion> ReportTemplateVersions => Set<ReportTemplateVersion>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditRules();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditRules();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(PersistenceSchemaNames.Business);

        modelBuilder.Entity<ReportTemplate>(entity =>
        {
            entity.ToTable("ReportTemplates");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.ModuleKey).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Type).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => new { x.ModuleKey, x.Status });
        });

        modelBuilder.Entity<ReportTemplateVersion>(entity =>
        {
            entity.ToTable("ReportTemplateVersions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TemplateJson).HasColumnType("text").IsRequired();
            entity.Property(x => x.SampleInputJson).HasColumnType("text").IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(2000).IsRequired();
            entity.HasOne<ReportTemplate>().WithMany().HasForeignKey(x => x.ReportTemplateId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.ReportTemplateId, x.VersionNumber }).IsUnique();
            entity.HasIndex(x => new { x.ReportTemplateId, x.IsPublished });
        });
    }

    private void ApplyAuditRules()
    {
        var now = DateTime.UtcNow;
        var actorId = auditActorAccessor.GetActorId();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not IAuditableEntity auditableEntity)
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                auditableEntity.CreatedAt = now;
                auditableEntity.CreatedBy ??= actorId;
            }
            else if (entry.State == EntityState.Modified)
            {
                auditableEntity.ModifiedAt = now;
                auditableEntity.ModifiedBy = actorId;
            }

            if (entry.State == EntityState.Deleted && entry.Entity is ISoftDeletable softDeletable)
            {
                entry.State = EntityState.Modified;
                softDeletable.IsDeleted = true;
                softDeletable.DeletedAt = now;
                softDeletable.DeletedBy = actorId;
            }
        }
    }
}
