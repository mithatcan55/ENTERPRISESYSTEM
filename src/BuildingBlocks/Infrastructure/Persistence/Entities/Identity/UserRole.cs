using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Identity;

/// <summary>
/// Kullanıcı-rol eşlemesi.
/// </summary>
public sealed class UserRole : AuditableIntEntity
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
}
