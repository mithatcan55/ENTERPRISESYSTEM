using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Materials;
/// <summary>
/// Material master kaydının çekirdek alanları.
/// Stok, lot ve seri hareketleri bilerek bu entity'ye konulmaz; onlar ayrı modülde yaşar.
/// </summary>
public sealed class Material : ActivatableAuditableIntEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int BaseUnitId { get; set; }
    public MaterialUnit? BaseUnit { get; set; }
    public int? SecondaryUnitId { get; set; }
    public MaterialUnit? SecondaryUnit { get; set; }
    public int? MaterialGroupId { get; set; }
    public MaterialGroup? MaterialGroup { get; set; }
    public string? DefaultShelfLocation { get; set; }
    public bool IsBatchTracked { get; set; }
    public bool IsSerialTracked { get; set; }
}
