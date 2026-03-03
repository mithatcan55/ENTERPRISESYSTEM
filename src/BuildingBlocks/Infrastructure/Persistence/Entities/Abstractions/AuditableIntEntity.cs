using SharedKernel.Auditing;

namespace Infrastructure.Persistence.Entities.Abstractions;

/// <summary>
/// Integer anahtar kullanan tüm ilişkisel entity'ler için ortak denetim yüzeyi.
/// Soft-delete alanlarını ve kim-ne-zaman bilgisini zorunlu kılar.
/// </summary>
public abstract class AuditableIntEntity : IAuditableEntity, ISoftDeletable
{
    public int Id { get; set; }

    public bool IsDeleted { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
}
