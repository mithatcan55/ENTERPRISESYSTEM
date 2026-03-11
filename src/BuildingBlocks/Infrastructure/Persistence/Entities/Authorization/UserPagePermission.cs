using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Authorization;

/// <summary>
/// Kullanıcıya 3. seviye ekran erişimi veren kayıt.
/// SYS01, SYS02 gibi T-Code çalışan sayfa erişimi bu tabloda tutulur.
/// </summary>
public sealed class UserPagePermission : AuditableIntEntity
{
    public int UserId { get; set; }
    public int SubModulePageId { get; set; }
    public short AuthorizationLevel { get; set; } = 3;
}
