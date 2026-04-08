using System.ComponentModel.DataAnnotations.Schema;
using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Identity;

/// <summary>
/// Sistem kullanıcı ana kaydı.
/// UserCode tek kimlik alanıdır. Username alanı kaldırılmıştır.
/// </summary>
public sealed class User : AuditableIntEntity
{
    public string UserCode { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; } = true;
    public DateTime? PasswordExpiresAt { get; set; }
    public string? ProfileImageUrl { get; set; }

    [NotMapped]
    public string DisplayName =>
        string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
            ? UserCode
            : $"{FirstName} {LastName}".Trim();
}
