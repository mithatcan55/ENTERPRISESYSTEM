using Approvals.Application.Commands;
using Approvals.Application.Contracts;
using Application.Observability;
using Application.Exceptions;
using global::Infrastructure.Persistence;
using global::Infrastructure.Persistence.Entities.Approvals;

namespace Approvals.Infrastructure.Delegations.Commands;

public sealed class CreateDelegationAssignmentCommandHandler(
    ApprovalsDbContext dbContext,
    IOperationalEventPublisher operationalEventPublisher) : ICreateDelegationAssignmentCommandHandler
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
            RevokedReason = string.Empty,
            Notes = request.Notes.Trim()
        };

        dbContext.DelegationAssignments.Add(assignment);
        await dbContext.SaveChangesAsync(cancellationToken);

        await operationalEventPublisher.PublishAsync(new OperationalEvent
        {
            EventName = "approval.delegation.created",
            Category = "ApprovalDelegation",
            Severity = "Information",
            Action = "Create",
            Resource = "DelegationAssignment",
            Message = $"Delegation olusturuldu. Delegator={assignment.DelegatorUserId}, Delegate={assignment.DelegateUserId}",
            UserId = assignment.DelegatorUserId.ToString(),
            Properties =
            {
                ["delegationAssignmentId"] = assignment.Id,
                ["delegatorUserId"] = assignment.DelegatorUserId,
                ["delegateUserId"] = assignment.DelegateUserId,
                ["scopeType"] = assignment.ScopeType,
                ["startsAt"] = assignment.StartsAt,
                ["endsAt"] = assignment.EndsAt
            }
        }, cancellationToken);

        return assignment.ToDetailDto();
    }
}
