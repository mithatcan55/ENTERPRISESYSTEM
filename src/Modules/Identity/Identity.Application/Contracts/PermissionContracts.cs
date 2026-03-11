namespace Identity.Application.Contracts;

public sealed class UpsertUserActionPermissionRequest
{
    public int UserId { get; set; }
    public int? SubModulePageId { get; set; }
    public string? TransactionCode { get; set; }
    public string ActionCode { get; set; } = string.Empty;
    public bool IsAllowed { get; set; } = true;
}

public sealed class UserActionPermissionQueryRequest
{
    public int UserId { get; set; }
    public int? SubModulePageId { get; set; }
    public string? TransactionCode { get; set; }
}

public sealed record UserActionPermissionDto(
    int Id,
    int UserId,
    int SubModulePageId,
    string TransactionCode,
    string ActionCode,
    bool IsAllowed,
    DateTime CreatedAt,
    DateTime? ModifiedAt);
