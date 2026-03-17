using Infrastructure.Persistence.Auditing;
using Infrastructure.Persistence.Entities.Documents;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Auditing;

namespace Infrastructure.Persistence;

public sealed class DocumentsDbContext(
    DbContextOptions<DocumentsDbContext> options,
    IAuditActorAccessor auditActorAccessor) : DbContext(options)
{
    public DbSet<ManagedDocument> ManagedDocuments => Set<ManagedDocument>();
    public DbSet<ManagedDocumentVersion> ManagedDocumentVersions => Set<ManagedDocumentVersion>();
    public DbSet<DocumentAssociation> DocumentAssociations => Set<DocumentAssociation>();

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

        modelBuilder.Entity<ManagedDocument>(entity =>
        {
            entity.ToTable("ManagedDocuments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(300).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.DocumentType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => new { x.DocumentType, x.Status });
        });

        modelBuilder.Entity<ManagedDocumentVersion>(entity =>
        {
            entity.ToTable("ManagedDocumentVersions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FileName).HasMaxLength(500).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(200).IsRequired();
            entity.Property(x => x.StoragePath).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.Checksum).HasMaxLength(200);
            entity.Property(x => x.ChangeNote).HasMaxLength(2000);
            entity.HasOne<ManagedDocument>().WithMany().HasForeignKey(x => x.ManagedDocumentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.ManagedDocumentId, x.VersionNumber }).IsUnique();
            entity.HasIndex(x => new { x.ManagedDocumentId, x.IsCurrent });
        });

        modelBuilder.Entity<DocumentAssociation>(entity =>
        {
            entity.ToTable("DocumentAssociations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OwnerEntityName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.OwnerEntityId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LinkType).HasMaxLength(100).IsRequired();
            entity.HasOne<ManagedDocument>().WithMany().HasForeignKey(x => x.ManagedDocumentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.OwnerEntityName, x.OwnerEntityId, x.LinkType });
            entity.HasIndex(x => new { x.ManagedDocumentId, x.OwnerEntityName, x.OwnerEntityId, x.LinkType }).IsUnique();
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
