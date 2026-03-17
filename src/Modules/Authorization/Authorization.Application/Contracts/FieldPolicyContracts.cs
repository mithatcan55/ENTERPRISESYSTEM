namespace Authorization.Application.Contracts;

public sealed record AuthorizationFieldDefinitionDto(
    int Id,
    string EntityName,
    string FieldName,
    string DisplayName,
    string DataType,
    IReadOnlyList<string> AllowedSurfaces,
    bool DefaultVisible,
    bool DefaultEditable,
    bool DefaultFilterable,
    bool DefaultExportable,
    bool IsActive,
    string? Description,
    DateTime CreatedAt,
    DateTime? ModifiedAt);

public sealed record UpsertAuthorizationFieldDefinitionRequest(
    int? Id,
    string EntityName,
    string FieldName,
    string DisplayName,
    string DataType,
    IReadOnlyList<string>? AllowedSurfaces,
    bool DefaultVisible,
    bool DefaultEditable,
    bool DefaultFilterable,
    bool DefaultExportable,
    bool IsActive,
    string? Description);

public sealed record AuthorizationFieldPolicyDto(
    int Id,
    string Name,
    string EntityName,
    string FieldName,
    string Surface,
    string TargetType,
    string? TargetKey,
    string Effect,
    string? ConditionFieldName,
    string ConditionOperator,
    string? CompareValue,
    string? MaskingMode,
    int Priority,
    bool IsActive,
    string? Description,
    DateTime CreatedAt,
    DateTime? ModifiedAt);

public sealed record UpsertAuthorizationFieldPolicyRequest(
    int? Id,
    string Name,
    string EntityName,
    string FieldName,
    string Surface,
    string TargetType,
    string? TargetKey,
    string Effect,
    string? ConditionFieldName,
    string ConditionOperator,
    string? CompareValue,
    string? MaskingMode,
    int Priority,
    bool IsActive,
    string? Description);

public sealed record EvaluateAuthorizationFieldPolicyRequest(
    string EntityName,
    string Surface,
    IReadOnlyDictionary<string, string?> FieldValues);

public sealed record AuthorizationFieldPolicyDecisionDto(
    string EntityName,
    string FieldName,
    string DisplayName,
    bool Visible,
    bool Editable,
    bool Filterable,
    bool Exportable,
    bool Masked,
    string? MaskingMode);
