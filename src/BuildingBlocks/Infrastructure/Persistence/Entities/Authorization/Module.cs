using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Authorization;

/// <summary>
/// 1. seviye yetkilendirme kabı.
/// Uygulamadaki ana modülü temsil eder (örn: System, Finance, HR).
/// </summary>
public sealed class Module : AuditableIntEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CompanyId { get; set; }
    public string? RouteLink { get; set; }
}
