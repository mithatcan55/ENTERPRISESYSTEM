using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Authorization;

/// <summary>
/// 6. seviye koşul yetkisi.
/// Örn: price <= 10000 gibi veri filtresi kuralları bu tabloda tutulur.
/// </summary>
public sealed class UserPageConditionPermission : AuditableIntEntity
{
    public int UserId { get; set; }
    public int SubModulePageId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public short AuthorizationLevel { get; set; } = 6;
}
