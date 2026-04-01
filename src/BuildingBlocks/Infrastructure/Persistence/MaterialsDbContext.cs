using Infrastructure.Persistence.Auditing;
using Infrastructure.Persistence.Entities.Materials;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Auditing;

namespace Infrastructure.Persistence;

/// <summary>
/// Materials bounded context'i için ayrı DbContext.
/// Bu ayırma, material alanını ileride documents, stock ve approvals ile büyütürken
/// migration ve tablo yönetimini kontrollü tutmamızı sağlar.
/// </summary>
public sealed class MaterialsDbContext(
    DbContextOptions<MaterialsDbContext> options,
    IAuditActorAccessor auditActorAccessor) : DbContext(options)
{
    public DbSet<Material> Materials => Set<Material>();

    public DbSet<MaterialUnit> MaterialUnits => Set<MaterialUnit>();

    public DbSet<MaterialGroup> MaterialGroups => Set<MaterialGroup>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        AuditRulesApplicator.Apply(ChangeTracker, auditActorAccessor);
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        AuditRulesApplicator.Apply(ChangeTracker, auditActorAccessor);
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(PersistenceSchemaNames.Business);

        ConfigureMaterialUnit(modelBuilder);
        ConfigureMaterialGroup(modelBuilder);
        ConfigureMaterial(modelBuilder);
    }

    private static void ConfigureMaterialUnit(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MaterialUnit>(entity =>
        {
            entity.ToTable("MaterialUnits");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                  .HasMaxLength(50)
                  .IsRequired();

            entity.Property(x => x.Name)
                  .HasMaxLength(200)
                  .IsRequired();

            entity.Property(x => x.Symbol)
                  .HasMaxLength(20)
                  .IsRequired();

            entity.HasIndex(x => x.Code)
                  .IsUnique();

            entity.HasIndex(x => new { x.IsActive, x.Name });


        });
    }

    private static void ConfigureMaterialGroup(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MaterialGroup>(entity =>
        {
            entity.ToTable("MaterialGroups");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                  .HasMaxLength(50)
                  .IsRequired();

            entity.Property(x => x.Name)
                  .HasMaxLength(200)
                  .IsRequired();

            entity.Property(x => x.Description)
                  .HasMaxLength(1000);

            entity.HasIndex(x => x.Code)
                  .IsUnique();

            entity.HasIndex(x => new { x.IsActive, x.Name });


        });
    }

    private static void ConfigureMaterial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Material>(entity =>
        {
            entity.ToTable("Materials");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(x => x.Name)
                  .HasMaxLength(300)
                  .IsRequired();

            entity.Property(x => x.Description)
                  .HasMaxLength(2000);

            entity.Property(x => x.DefaultShelfLocation)
                  .HasMaxLength(100);

            entity.HasOne<MaterialUnit>()
                  .WithMany()
                  .HasForeignKey(x => x.BaseUnitId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<MaterialUnit>()
                  .WithMany()
                  .HasForeignKey(x => x.SecondaryUnitId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<MaterialGroup>()
                  .WithMany()
                  .HasForeignKey(x => x.MaterialGroupId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.Code)
                  .IsUnique();


            entity.HasIndex(x => new { x.MaterialGroupId, x.IsActive });
        });
    }
}