namespace Identity.Application.Contracts;

public sealed record UserPermissionSummaryDto(
    int UserId,
    IReadOnlyList<UserModuleAccessDto> Modules,
    IReadOnlyList<UserCompanyAccessDto> Companies,
    IReadOnlyList<UserActionPermissionDto> ActionPermissions);

public sealed record UserModuleAccessDto(
    int ModuleId,
    string ModuleCode,
    string ModuleName,
    bool HasAccess,
    IReadOnlyList<UserSubModuleAccessDto> SubModules);

public sealed record UserSubModuleAccessDto(
    int SubModuleId,
    string SubModuleCode,
    string SubModuleName,
    bool HasAccess,
    IReadOnlyList<UserPageAccessDto> Pages);

public sealed record UserPageAccessDto(
    int PageId,
    string PageCode,
    string PageName,
    string TransactionCode,
    bool HasAccess,
    IReadOnlyList<string> AllowedActions);

public sealed record UserCompanyAccessDto(
    int CompanyId,
    bool HasAccess);

// UserActionPermissionDto is defined in PermissionContracts.cs

public sealed class GrantUserPermissionsRequest
{
    public IReadOnlyList<int>? ModuleIds { get; set; }
    public IReadOnlyList<int>? SubModuleIds { get; set; }
    public IReadOnlyList<int>? PageIds { get; set; }
    public IReadOnlyList<int>? CompanyIds { get; set; }
    public bool RevokeOthers { get; set; }
}
