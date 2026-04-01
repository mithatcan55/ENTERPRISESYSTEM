using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Abstractions;

/// <summary>
/// Aktif/pasif business durumu tasiyan master veriler icin ortak taban.
/// Soft delete ile ayni sey degildir; kayit sistemde durur ama yeni kullanimlara kapatilabilir.
/// </summary>
public abstract class ActivatableAuditableIntEntity : AuditableIntEntity
{
    public bool IsActive { get; set; } = true;
}
