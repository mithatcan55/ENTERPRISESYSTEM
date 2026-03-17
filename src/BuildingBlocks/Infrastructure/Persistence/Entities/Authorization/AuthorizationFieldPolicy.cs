using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Authorization;

/// <summary>
/// Bir field icin kullanici/rol/permission bazli dinamik davranis tanimini tutar.
/// Ornek: Material.Price alani tablo yuzeyinde gizli olsun veya 10000 uzeri degerlerde maskelensin.
/// </summary>
public sealed class AuthorizationFieldPolicy : AuditableIntEntity
{
    public string Name { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string Surface { get; set; } = "ANY";
    public string TargetType { get; set; } = "ANY";
    public string? TargetKey { get; set; }
    public string Effect { get; set; } = string.Empty;
    public string? ConditionFieldName { get; set; }
    public string ConditionOperator { get; set; } = "ALWAYS";
    public string? CompareValue { get; set; }
    public string? MaskingMode { get; set; }
    public int Priority { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
