using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Documents;

/// <summary>
/// Dokumanin mantiksal kaydidir.
/// Fiziksel dosya versiyonlari ayri tabloda tutulur.
/// </summary>
public sealed class ManagedDocument : AuditableIntEntity
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
