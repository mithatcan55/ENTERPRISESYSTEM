using SharedKernel.Domain;

namespace SharedKernel.Auditing;

public abstract class AuditableEntity : Entity, IAuditableEntity, ISoftDeletable
{
    public Guid Id { get; protected set; }

    public bool IsDeleted { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }

    // PostgreSQL'de gerçek rowversion tipi yok, app-level concurrency için versiyon alanı.
    public long Version { get; set; }
}
