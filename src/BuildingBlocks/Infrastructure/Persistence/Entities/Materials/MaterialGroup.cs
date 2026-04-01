using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Materials;
/// <summary>
/// Material kayıtlarını mantıksal gruplara ayırmak için kullanılır.
/// İlk fazda hiyerarşi yok; tek seviye grup yapısı yeterli.
/// </summary>
public sealed class MaterialGroup : ActivatableAuditableIntEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

}
