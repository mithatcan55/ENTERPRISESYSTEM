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
    public DbSet<AuthorizationFieldDefinition> AuthorizationFieldDefinitions => Set<AuthorizationFieldDefinition>();
    public DbSet<AuthorizationFieldPolicy> AuthorizationFieldPolicies => Set<AuthorizationFieldPolicy>();

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
    }
}
