using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Identity;

/// <summary>
/// Kullanıcı oturum kaydı. Aktif/revoke izleme için kullanılır.
/// </summary>
public sealed class UserSession : AuditableIntEntity
{
    public int UserId { get; set; }
    public string SessionKey { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedBy { get; set; }
    public string? ClientIpAddress { get; set; }
    public string? UserAgent { get; set; }
}
