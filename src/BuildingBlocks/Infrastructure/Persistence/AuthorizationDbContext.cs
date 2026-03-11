using Infrastructure.Persistence.Auditing;
using Infrastructure.Persistence.Entities.Authorization;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Auditing;

namespace Infrastructure.Persistence;

public sealed class AuthorizationDbContext(
    DbContextOptions<AuthorizationDbContext> options,
    IAuditActorAccessor auditActorAccessor) : DbContext(options)
{
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<SubModule> SubModules => Set<SubModule>();
    public DbSet<SubModulePage> SubModulePages => Set<SubModulePage>();
    public DbSet<UserModulePermission> UserModulePermissions => Set<UserModulePermission>();
    public DbSet<UserSubModulePermission> UserSubModulePermissions => Set<UserSubModulePermission>();
    public DbSet<UserPagePermission> UserPagePermissions => Set<UserPagePermission>();
    public DbSet<UserCompanyPermission> UserCompanyPermissions => Set<UserCompanyPermission>();
    public DbSet<UserPageActionPermission> UserPageActionPermissions => Set<UserPageActionPermission>();
    public DbSet<UserPageConditionPermission> UserPageConditionPermissions => Set<UserPageConditionPermission>();

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
        modelBuilder.HasDefaultSchema(BusinessDbContext.AuthorizationSchema);

        modelBuilder.Entity<Module>(entity =>
        {
            entity.ToTable("Modules");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.RouteLink).HasMaxLength(500);
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<SubModule>(entity =>
        {
            entity.ToTable("SubModules");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.RouteLink).HasMaxLength(500);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasOne<Module>().WithMany().HasForeignKey(x => x.ModuleId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SubModulePage>(entity =>
        {
            entity.ToTable("SubModulePages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(100).IsRequired();
            entity.Property(x => x.TransactionCode).HasMaxLength(20).IsRequired();
            entity.Property(x => x.RouteLink).HasMaxLength(500);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.TransactionCode).IsUnique();
            entity.HasOne<SubModule>().WithMany().HasForeignKey(x => x.SubModuleId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserModulePermission>(entity =>
        {
            entity.ToTable("UserModulePermissions");
            entity.HasKey(x => x.Id);
            entity.HasOne<Module>().WithMany().HasForeignKey(x => x.ModuleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.UserId, x.ModuleId }).IsUnique();
        });

        modelBuilder.Entity<UserSubModulePermission>(entity =>
        {
            entity.ToTable("UserSubModulePermissions");
            entity.HasKey(x => x.Id);
            entity.HasOne<SubModule>().WithMany().HasForeignKey(x => x.SubModuleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.UserId, x.SubModuleId }).IsUnique();
        });

        modelBuilder.Entity<UserPagePermission>(entity =>
        {
            entity.ToTable("UserPagePermissions");
            entity.HasKey(x => x.Id);
            entity.HasOne<SubModulePage>().WithMany().HasForeignKey(x => x.SubModulePageId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.UserId, x.SubModulePageId }).IsUnique();
        });

        modelBuilder.Entity<UserCompanyPermission>(entity =>
        {
            entity.ToTable("UserCompanyPermissions");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.CompanyId }).IsUnique();
        });

        modelBuilder.Entity<UserPageActionPermission>(entity =>
        {
            entity.ToTable("UserPageActionPermissions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ActionCode).HasMaxLength(200).IsRequired();
            entity.HasOne<SubModulePage>().WithMany().HasForeignKey(x => x.SubModulePageId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.UserId, x.SubModulePageId, x.ActionCode }).IsUnique();
        });

        modelBuilder.Entity<UserPageConditionPermission>(entity =>
        {
            entity.ToTable("UserPageConditionPermissions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FieldName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Operator).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Value).HasMaxLength(500).IsRequired();
            entity.HasOne<SubModulePage>().WithMany().HasForeignKey(x => x.SubModulePageId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.UserId, x.SubModulePageId, x.FieldName, x.Operator, x.Value });
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
