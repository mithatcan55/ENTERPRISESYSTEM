using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Authorization;

/// <summary>
/// Kullanıcıya 1. seviye modül erişimi veren kayıt.
/// </summary>
public sealed class UserModulePermission : AuditableIntEntity
{
    public int UserId { get; set; }
    public int ModuleId { get; set; }
    public short AuthorizationLevel { get; set; } = 1;
}
