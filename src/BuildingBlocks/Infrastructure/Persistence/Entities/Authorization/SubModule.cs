using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Authorization;

/// <summary>
/// 2. seviye yetkilendirme kabı.
/// Bir modülün alt iş alanını temsil eder.
/// </summary>
public sealed class SubModule : AuditableIntEntity
{
    public int ModuleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? RouteLink { get; set; }
}
