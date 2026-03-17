namespace Approvals.Application.Contracts;

public sealed record DelegationAssignmentQueryRequest(bool? IsActive = null, int Page = 1, int PageSize = 20);

public sealed record DelegationAssignmentListItemDto(
    int Id,
    int DelegatorUserId,
    int DelegateUserId,
    string ScopeType,
    DateTime StartsAt,
    DateTime EndsAt,
    bool IsActive,
    int? RevokedByUserId,
    DateTime? RevokedAt,
    string RevokedReason,
    string Notes);

public sealed record DelegationAssignmentDetailDto(
    int Id,
    int DelegatorUserId,
    int DelegateUserId,
    string ScopeType,
    string IncludedScopesJson,
    string ExcludedScopesJson,
    DateTime StartsAt,
    DateTime EndsAt,
    bool IsActive,
    int? RevokedByUserId,
    DateTime? RevokedAt,
    string RevokedReason,
    string Notes);

public sealed record CreateDelegationAssignmentRequest(
    int DelegatorUserId,
    int DelegateUserId,
    string ScopeType,
    string IncludedScopesJson,
    string ExcludedScopesJson,
    DateTime StartsAt,
    DateTime EndsAt,
    string Notes);

public sealed record SetDelegationAssignmentStatusRequest(
    bool IsActive,
    string Reason);
