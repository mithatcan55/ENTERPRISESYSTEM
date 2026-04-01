using Infrastructure.Persistence.Auditing;
using Infrastructure.Persistence.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Auditing;

namespace Infrastructure.Persistence;

public sealed class IdentityDbContext(
    DbContextOptions<IdentityDbContext> options,
    IAuditActorAccessor auditActorAccessor) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<UserPasswordHistory> UserPasswordHistories => Set<UserPasswordHistory>();
    public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();

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

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Username).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(320).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(x => x.ProfileImageUrl).HasMaxLength(2000);
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
            entity.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Role>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
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
            entity.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
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
            entity.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<UserSession>().WithMany().HasForeignKey(x => x.UserSessionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => x.TokenId).IsUnique();
            entity.HasIndex(x => new { x.UserId, x.UserSessionId, x.IsRevoked, x.ExpiresAt });
        });

        modelBuilder.Entity<UserPasswordHistory>(entity =>
        {
            entity.ToTable("UserPasswordHistories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.UserId, x.ChangedAt });
        });
    }
}
