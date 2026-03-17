using Approvals.Application.Contracts;
using global::Infrastructure.Persistence.Entities.Approvals;

namespace Approvals.Infrastructure.Delegations;

internal static class DelegationMappingExtensions
{
    public static DelegationAssignmentListItemDto ToListItemDto(this DelegationAssignment entity)
        => new(
            entity.Id,
            entity.DelegatorUserId,
            entity.DelegateUserId,
            entity.ScopeType,
            entity.StartsAt,
            entity.EndsAt,
            entity.IsActive,
            entity.RevokedByUserId,
            entity.RevokedAt,
            entity.RevokedReason,
            entity.Notes);

    public static DelegationAssignmentDetailDto ToDetailDto(this DelegationAssignment entity)
        => new(
            entity.Id,
            entity.DelegatorUserId,
            entity.DelegateUserId,
            entity.ScopeType,
            entity.IncludedScopesJson,
            entity.ExcludedScopesJson,
            entity.StartsAt,
            entity.EndsAt,
            entity.IsActive,
            entity.RevokedByUserId,
            entity.RevokedAt,
            entity.RevokedReason,
            entity.Notes);
}
