using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Authorization;

/// <summary>
/// Kullanıcıya 2. seviye alt modül erişimi veren kayıt.
/// </summary>
public sealed class UserSubModulePermission : AuditableIntEntity
{
    public int UserId { get; set; }
    public int SubModuleId { get; set; }
    public short AuthorizationLevel { get; set; } = 2;
}
