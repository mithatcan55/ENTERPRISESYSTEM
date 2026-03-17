using Infrastructure.Persistence.Auditing;
using Infrastructure.Persistence.Entities.Approvals;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Auditing;

namespace Infrastructure.Persistence;

public sealed class ApprovalsDbContext(
    DbContextOptions<ApprovalsDbContext> options,
    IAuditActorAccessor auditActorAccessor) : DbContext(options)
{
    public DbSet<ApprovalWorkflowDefinition> ApprovalWorkflowDefinitions => Set<ApprovalWorkflowDefinition>();
    public DbSet<ApprovalWorkflowStep> ApprovalWorkflowSteps => Set<ApprovalWorkflowStep>();
    public DbSet<ApprovalWorkflowCondition> ApprovalWorkflowConditions => Set<ApprovalWorkflowCondition>();
    public DbSet<ApprovalInstance> ApprovalInstances => Set<ApprovalInstance>();
    public DbSet<ApprovalInstanceStep> ApprovalInstanceSteps => Set<ApprovalInstanceStep>();
    public DbSet<ApprovalDecision> ApprovalDecisions => Set<ApprovalDecision>();
    public DbSet<DelegationAssignment> DelegationAssignments => Set<DelegationAssignment>();

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

        modelBuilder.Entity<ApprovalWorkflowDefinition>(entity =>
        {
            entity.ToTable("ApprovalWorkflowDefinitions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.ModuleKey).HasMaxLength(100).IsRequired();
            entity.Property(x => x.DocumentType).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => new { x.ModuleKey, x.DocumentType, x.IsActive });
        });

        modelBuilder.Entity<ApprovalWorkflowStep>(entity =>
        {
            entity.ToTable("ApprovalWorkflowSteps");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ApproverType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ApproverValue).HasMaxLength(300).IsRequired();
            entity.Property(x => x.TimeoutDecision).HasMaxLength(50).IsRequired();
            entity.HasOne<ApprovalWorkflowDefinition>().WithMany().HasForeignKey(x => x.ApprovalWorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.ApprovalWorkflowDefinitionId, x.StepOrder }).IsUnique();
        });

        modelBuilder.Entity<ApprovalWorkflowCondition>(entity =>
        {
            entity.ToTable("ApprovalWorkflowConditions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FieldKey).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Operator).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Value).HasMaxLength(1000).IsRequired();
            entity.HasOne<ApprovalWorkflowDefinition>().WithMany().HasForeignKey(x => x.ApprovalWorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.ApprovalWorkflowDefinitionId, x.FieldKey, x.Operator });
        });

        modelBuilder.Entity<ApprovalInstance>(entity =>
        {
            entity.ToTable("ApprovalInstances");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReferenceType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ReferenceId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("text").IsRequired();
            entity.HasOne<ApprovalWorkflowDefinition>().WithMany().HasForeignKey(x => x.ApprovalWorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.ReferenceType, x.ReferenceId, x.Status });
        });

        modelBuilder.Entity<ApprovalInstanceStep>(entity =>
        {
            entity.ToTable("ApprovalInstanceSteps");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
            entity.HasOne<ApprovalInstance>().WithMany().HasForeignKey(x => x.ApprovalInstanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ApprovalWorkflowStep>().WithMany().HasForeignKey(x => x.ApprovalWorkflowStepId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.ApprovalInstanceId, x.StepOrder });
            entity.HasIndex(x => new { x.AssignedUserId, x.Status });
        });

        modelBuilder.Entity<ApprovalDecision>(entity =>
        {
            entity.ToTable("ApprovalDecisions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Decision).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Comment).HasMaxLength(2000).IsRequired();
            entity.HasOne<ApprovalInstanceStep>().WithMany().HasForeignKey(x => x.ApprovalInstanceStepId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.ApprovalInstanceStepId, x.ActorUserId, x.CreatedAt });
            entity.HasIndex(x => new { x.IsSystemDecision, x.Decision, x.CreatedAt });
        });

        modelBuilder.Entity<DelegationAssignment>(entity =>
        {
            entity.ToTable("DelegationAssignments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ScopeType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.IncludedScopesJson).HasColumnType("text").IsRequired();
            entity.Property(x => x.ExcludedScopesJson).HasColumnType("text").IsRequired();
            entity.Property(x => x.RevokedReason).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(2000).IsRequired();
            entity.HasIndex(x => new { x.DelegatorUserId, x.DelegateUserId, x.IsActive, x.EndsAt });
            entity.HasIndex(x => new { x.DelegatorUserId, x.IsActive, x.StartsAt, x.EndsAt });
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
