using Infrastructure.Persistence.Entities.Abstractions;

namespace Infrastructure.Persistence.Entities.Authorization;

/// <summary>
/// Ekran ve API tarafinda kullanilan alanlarin merkezi metadata kaydidir.
/// Dynamic policy motoru hangi field'in hangi surface'te davranis uretecegini
/// bu tanimlar uzerinden bilir.
/// </summary>
public sealed class AuthorizationFieldDefinition : AuditableIntEntity
{
    public string EntityName { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string? AllowedSurfaces { get; set; }
    public string? Description { get; set; }
    public bool DefaultVisible { get; set; } = true;
    public bool DefaultEditable { get; set; } = true;
    public bool DefaultFilterable { get; set; } = true;
    public bool DefaultExportable { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
