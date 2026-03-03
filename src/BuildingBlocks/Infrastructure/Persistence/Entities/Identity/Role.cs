using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Identity;

/// <summary>
/// Kullanıcıların atanabildiği sistem rolü.
/// </summary>
public sealed class Role : AuditableIntEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
}
