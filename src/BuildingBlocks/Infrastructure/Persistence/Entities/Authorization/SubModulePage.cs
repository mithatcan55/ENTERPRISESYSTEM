using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Authorization;

/// <summary>
/// 3. seviye yetkilendirme birimi.
/// Somut ekranı/fonksiyonu temsil eder.
/// TransactionCode alanı SAP/CANIAS benzeri doğrudan ekran çağırma için tutulur (örn: SYS01).
/// </summary>
public sealed class SubModulePage : AuditableIntEntity
{
    public int SubModuleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string TransactionCode { get; set; } = string.Empty;
    public string? RouteLink { get; set; }
}
