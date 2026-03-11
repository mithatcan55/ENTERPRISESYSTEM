using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Identity;

public sealed class UserRefreshToken : AuditableIntEntity
{
    public int UserId { get; set; }
    public int UserSessionId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public string TokenId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedBy { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public DateTime? ReuseDetectedAt { get; set; }
    public string? CreatedByIp { get; set; }
    public string? UserAgent { get; set; }
}
