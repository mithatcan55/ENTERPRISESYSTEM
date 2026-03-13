using Approvals.Application.Commands;
using Approvals.Application.Contracts;
using Application.Exceptions;
using global::Infrastructure.Persistence;
using global::Infrastructure.Persistence.Entities.Approvals;

namespace Approvals.Infrastructure.Delegations.Commands;

public sealed class CreateDelegationAssignmentCommandHandler(ApprovalsDbContext dbContext) : ICreateDelegationAssignmentCommandHandler
{
    public async Task<DelegationAssignmentDetailDto> HandleAsync(CreateDelegationAssignmentRequest request, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.DelegatorUserId <= 0) errors["delegatorUserId"] = ["Delegator user zorunludur."];
        if (request.DelegateUserId <= 0) errors["delegateUserId"] = ["Delegate user zorunludur."];
        if (request.DelegatorUserId == request.DelegateUserId) errors["delegateUserId"] = ["Kullanici kendi kendine vekalet veremez."];
        if (request.EndsAt <= request.StartsAt) errors["endsAt"] = ["Bitis tarihi baslangictan buyuk olmalidir."];
        if (string.IsNullOrWhiteSpace(request.ScopeType)) errors["scopeType"] = ["Scope type zorunludur."];

        if (errors.Count > 0)
        {
            throw new ValidationAppException("Delegation assignment request gecersiz.", errors);
        }

        var assignment = new DelegationAssignment
        {
            DelegatorUserId = request.DelegatorUserId,
            DelegateUserId = request.DelegateUserId,
            ScopeType = request.ScopeType.Trim(),
            IncludedScopesJson = string.IsNullOrWhiteSpace(request.IncludedScopesJson) ? "[]" : request.IncludedScopesJson.Trim(),
            ExcludedScopesJson = string.IsNullOrWhiteSpace(request.ExcludedScopesJson) ? "[]" : request.ExcludedScopesJson.Trim(),
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            IsActive = true,
            Notes = request.Notes.Trim()
        };

        dbContext.DelegationAssignments.Add(assignment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return assignment.ToDetailDto();
    }
}
