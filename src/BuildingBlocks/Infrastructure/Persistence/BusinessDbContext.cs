using Infrastructure.Persistence.Auditing;
using Infrastructure.Persistence.Entities.Authorization;
using Infrastructure.Persistence.Entities.Integration;
using Infrastructure.Persistence.Entities.Identity;
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
    public DbSet<AuthorizationFieldDefinition> AuthorizationFieldDefinitions => Set<AuthorizationFieldDefinition>();
    public DbSet<AuthorizationFieldPolicy> AuthorizationFieldPolicies => Set<AuthorizationFieldPolicy>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<UserPasswordHistory> UserPasswordHistories => Set<UserPasswordHistory>();
    public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();
    public DbSet<ExternalOutboxMessage> ExternalOutboxMessages => Set<ExternalOutboxMessage>();

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

        modelBuilder.Entity<AuthorizationFieldDefinition>(entity =>
        {
            entity.ToTable("AuthorizationFieldDefinitions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EntityName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.FieldName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.DataType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.AllowedSurfaces).HasMaxLength(1000);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.HasIndex(x => new { x.EntityName, x.FieldName }).IsUnique();
        });

        modelBuilder.Entity<AuthorizationFieldPolicy>(entity =>
        {
            entity.ToTable("AuthorizationFieldPolicies");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.EntityName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.FieldName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Surface).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TargetType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TargetKey).HasMaxLength(200);
            entity.Property(x => x.Effect).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ConditionFieldName).HasMaxLength(200);
            entity.Property(x => x.ConditionOperator).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CompareValue).HasMaxLength(500);
            entity.Property(x => x.MaskingMode).HasMaxLength(50);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.HasIndex(x => new { x.EntityName, x.FieldName, x.Surface, x.Priority });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Username).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(320).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.HasIndex(x => x.UserCode).IsUnique();
            entity.HasIndex(x => x.Username).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(x => x.Id);
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Role>()
                .WithMany()
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.ToTable("UserSessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SessionKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ClientIpAddress).HasMaxLength(100);
            entity.Property(x => x.UserAgent).HasMaxLength(1000);
            entity.Property(x => x.RevokedBy).HasMaxLength(200);
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.SessionKey).IsUnique();
            entity.HasIndex(x => new { x.UserId, x.IsRevoked, x.ExpiresAt });
        });

        modelBuilder.Entity<UserRefreshToken>(entity =>
        {
            entity.ToTable("UserRefreshTokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).HasMaxLength(200).IsRequired();
            entity.Property(x => x.TokenId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.RevokedBy).HasMaxLength(200);
            entity.Property(x => x.ReplacedByTokenHash).HasMaxLength(200);
            entity.Property(x => x.CreatedByIp).HasMaxLength(100);
            entity.Property(x => x.UserAgent).HasMaxLength(1000);
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<UserSession>()
                .WithMany()
                .HasForeignKey(x => x.UserSessionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => x.TokenId).IsUnique();
            entity.HasIndex(x => new { x.UserId, x.UserSessionId, x.IsRevoked, x.ExpiresAt });
        });

        modelBuilder.Entity<UserPasswordHistory>(entity =>
        {
            entity.ToTable("UserPasswordHistories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.UserId, x.ChangedAt });
        });

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

        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = 1,
                Code = "SYS_ADMIN",
                Name = "System Administrator",
                Description = "Tam yetkili sistem yöneticisi",
                IsSystemRole = true,
                CreatedBy = "seed",
                CreatedAt = seedCreatedAt
            },
            new Role
            {
                Id = 2,
                Code = "SYS_OPERATOR",
                Name = "System Operator",
                Description = "Operasyonel kullanıcı",
                IsSystemRole = true,
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


