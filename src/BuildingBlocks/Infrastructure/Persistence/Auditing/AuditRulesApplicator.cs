using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SharedKernel.Auditing;

namespace Infrastructure.Persistence.Auditing;

/// <summary>
/// Tüm DbContext'ler için ortak denetim kuralı uygulayıcısı.
/// CreatedBy/ModifiedBy/DeletedBy alanlarını EF değişiklik takibine göre otomatik doldurur.
/// Soft-delete: EntityState.Deleted → EntityState.Modified'e dönüştürülür, IsDeleted/DeletedAt/DeletedBy set edilir.
/// </summary>
public static class AuditRulesApplicator
{
    public static void Apply(ChangeTracker changeTracker, IAuditActorAccessor auditActorAccessor)
    {
        var now = DateTime.UtcNow;
        var actorId = auditActorAccessor.GetActorId();

        foreach (var entry in changeTracker.Entries())
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
