using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Authorization;

/// <summary>
/// 5. seviye işlem yetkisi.
/// Buton göster/gizle, pasif et, kolon göster gibi aksiyon bazlı haklar burada tutulur.
/// </summary>
public sealed class UserPageActionPermission : AuditableIntEntity
{
    public int UserId { get; set; }
    public int SubModulePageId { get; set; }
    public string ActionCode { get; set; } = string.Empty;
    public bool IsAllowed { get; set; }
    public short AuthorizationLevel { get; set; } = 5;
}
