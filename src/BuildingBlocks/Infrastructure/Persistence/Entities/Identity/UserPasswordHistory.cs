using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Identity;

/// <summary>
/// Kullanıcının geçmiş şifre hash kayıtları.
/// Password policy history/reuse kontrolleri bu tablo üzerinden yapılır.
/// </summary>
public sealed class UserPasswordHistory : AuditableIntEntity
{
    public int UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
