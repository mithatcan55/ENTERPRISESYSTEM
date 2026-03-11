using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Authorization;

/// <summary>
/// Kullanıcının şirket kapsamı erişimini tutar (4. seviye).
/// Aynı kullanıcı farklı şirketlerde farklı görünürlük alabilir.
/// </summary>
public sealed class UserCompanyPermission : AuditableIntEntity
{
    public int UserId { get; set; }
    public int CompanyId { get; set; }
    public short AuthorizationLevel { get; set; } = 4;
}
