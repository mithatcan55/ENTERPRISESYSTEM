using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Materials;
/// <summary>
/// Material ana verisinde kullanilan olcu birimlerini tutar.
/// Sabit veri gibi gorunse de UI uzerinden yonetilecegi icin entity olarak tutulur.
/// </summary>
public sealed class MaterialUnit : ActivatableAuditableIntEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public int DecimalPrecision { get; set; }


}

