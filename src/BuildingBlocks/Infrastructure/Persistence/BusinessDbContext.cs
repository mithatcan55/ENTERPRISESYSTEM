using Infrastructure.Persistence.Auditing;
using Infrastructure.Persistence.Entities.Authorization;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Auditing;

namespace Infrastructure.Persistence;

/// <summary>
/// Yetkilendirme tarafındaki tüm kurumsal tabloları yöneten DbContext.
/// Bu context özellikle SAP/CANIAS benzeri T-Code erişimi, 6 seviye yetki,
/// ve kullanıcı bazlı denetlenebilir değişiklik izi için tasarlanmıştır.
/// </summary>
public sealed class BusinessDbContext(
    DbContextOptions<BusinessDbContext> options,
    IAuditActorAccessor auditActorAccessor) : DbContext(options)
{
    public const string AuthorizationSchema = "authorizeSchema";

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
        var seedCreatedAt = new DateTime(2026, 03, 03, 00, 00, 00, DateTimeKind.Utc);

        modelBuilder.HasDefaultSchema(AuthorizationSchema);

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
            entity.HasOne<Module>()
                .WithMany()
                .HasForeignKey(x => x.ModuleId)
                .OnDelete(DeleteBehavior.Restrict);
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
            entity.HasOne<SubModule>()
                .WithMany()
                .HasForeignKey(x => x.SubModuleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserModulePermission>(entity =>
        {
            entity.ToTable("UserModulePermissions");
            entity.HasKey(x => x.Id);
            entity.HasOne<Module>()
                .WithMany()
                .HasForeignKey(x => x.ModuleId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.UserId, x.ModuleId }).IsUnique();
        });

        modelBuilder.Entity<UserSubModulePermission>(entity =>
        {
            entity.ToTable("UserSubModulePermissions");
            entity.HasKey(x => x.Id);
            entity.HasOne<SubModule>()
                .WithMany()
                .HasForeignKey(x => x.SubModuleId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.UserId, x.SubModuleId }).IsUnique();
        });

        modelBuilder.Entity<UserPagePermission>(entity =>
        {
            entity.ToTable("UserPagePermissions");
            entity.HasKey(x => x.Id);
            entity.HasOne<SubModulePage>()
                .WithMany()
                .HasForeignKey(x => x.SubModulePageId)
                .OnDelete(DeleteBehavior.Restrict);
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
            entity.HasOne<SubModulePage>()
                .WithMany()
                .HasForeignKey(x => x.SubModulePageId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.UserId, x.SubModulePageId, x.ActionCode }).IsUnique();
        });

        modelBuilder.Entity<UserPageConditionPermission>(entity =>
        {
            entity.ToTable("UserPageConditionPermissions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FieldName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Operator).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Value).HasMaxLength(500).IsRequired();
            entity.HasOne<SubModulePage>()
                .WithMany()
                .HasForeignKey(x => x.SubModulePageId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.UserId, x.SubModulePageId, x.FieldName, x.Operator, x.Value });
        });

        // SYS01-SYS04 başlangıç ekranları: T-Code ile doğrudan erişim sağlayan temel seed.
        modelBuilder.Entity<Module>().HasData(new Module
        {
            Id = 1,
            Name = "System",
            Code = "SYS",
            Description = "Sistem yönetimi ana modülü",
            CompanyId = 1,
            RouteLink = "/system",
            CreatedBy = "seed",
            CreatedAt = seedCreatedAt
        });

        modelBuilder.Entity<SubModule>().HasData(new SubModule
        {
            Id = 1,
            ModuleId = 1,
            Name = "UserManagement",
            Code = "SYS_USER",
            Description = "Kullanıcı işlemleri",
            RouteLink = "/system/users",
            CreatedBy = "seed",
            CreatedAt = seedCreatedAt
        });

        modelBuilder.Entity<SubModulePage>().HasData(
            new SubModulePage
            {
                Id = 1,
                SubModuleId = 1,
                Name = "Create User",
                Code = "USER_CREATE",
                TransactionCode = "SYS01",
                RouteLink = "/system/users/create",
                CreatedBy = "seed",
                CreatedAt = seedCreatedAt
            },
            new SubModulePage
            {
                Id = 2,
                SubModuleId = 1,
                Name = "Update User",
                Code = "USER_UPDATE",
                TransactionCode = "SYS02",
                RouteLink = "/system/users/update",
                CreatedBy = "seed",
                CreatedAt = seedCreatedAt
            },
            new SubModulePage
            {
                Id = 3,
                SubModuleId = 1,
                Name = "View User",
                Code = "USER_VIEW",
                TransactionCode = "SYS03",
                RouteLink = "/system/users/view",
                CreatedBy = "seed",
                CreatedAt = seedCreatedAt
            },
            new SubModulePage
            {
                Id = 4,
                SubModuleId = 1,
                Name = "User Report",
                Code = "USER_REPORT",
                TransactionCode = "SYS04",
                RouteLink = "/system/users/report",
                CreatedBy = "seed",
                CreatedAt = seedCreatedAt
            });

        base.OnModelCreating(modelBuilder);
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
