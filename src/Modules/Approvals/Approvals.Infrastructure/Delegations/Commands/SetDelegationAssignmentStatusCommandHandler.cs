using Approvals.Application.Commands;
using Approvals.Application.Contracts;
using Application.Exceptions;
using Application.Observability;
using Application.Security;
using global::Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Approvals.Infrastructure.Delegations.Commands;

public sealed class SetDelegationAssignmentStatusCommandHandler(
    ApprovalsDbContext approvalsDbContext,
    ICurrentUserContext currentUserContext,
    IOperationalEventPublisher operationalEventPublisher) : ISetDelegationAssignmentStatusCommandHandler
{
    public async Task<DelegationAssignmentDetailDto> HandleAsync(int delegationAssignmentId, SetDelegationAssignmentStatusRequest request, CancellationToken cancellationToken)
    {
        var assignment = await approvalsDbContext.DelegationAssignments
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == delegationAssignmentId, cancellationToken)
            ?? throw new NotFoundAppException($"Delegation assignment bulunamadi. Id={delegationAssignmentId}");

        var isSysAdmin = currentUserContext.IsInRole("SYS_ADMIN");
        var hasUserId = currentUserContext.TryGetUserId(out var actorUserId);

        if (!isSysAdmin && (!hasUserId || actorUserId != assignment.DelegatorUserId))
        {
            throw new ForbiddenAppException("Bu delegation kaydini sadece devreden kullanici veya SYS_ADMIN degistirebilir.");
        }

        if (request.IsActive && assignment.EndsAt <= DateTime.UtcNow)
        {
            throw new ValidationAppException("Suresi biten delegation yeniden aktif edilemez.", new Dictionary<string, string[]>
            {
                ["isActive"] = ["Bitmis delegation icin yeni kayit olusturulmalidir."]
            });
        }

        assignment.IsActive = request.IsActive;

        if (request.IsActive)
        {
            assignment.RevokedAt = null;
            assignment.RevokedByUserId = null;
            assignment.RevokedReason = string.Empty;
        }
        else
        {
            assignment.RevokedAt = DateTime.UtcNow;
            assignment.RevokedByUserId = hasUserId ? actorUserId : null;
            assignment.RevokedReason = request.Reason?.Trim() ?? string.Empty;
        }

        await approvalsDbContext.SaveChangesAsync(cancellationToken);

        await operationalEventPublisher.PublishAsync(new OperationalEvent
        {
            EventName = request.IsActive ? "approval.delegation.reactivated" : "approval.delegation.revoked",
            Category = "ApprovalDelegation",
            Severity = "Information",
            Action = request.IsActive ? "Reactivate" : "Revoke",
            Resource = "DelegationAssignment",
            Message = request.IsActive
                ? $"Delegation yeniden aktif edildi. Id={assignment.Id}"
                : $"Delegation geri alindi. Id={assignment.Id}",
            UserId = hasUserId ? actorUserId.ToString() : null,
            Properties =
            {
                ["delegationAssignmentId"] = assignment.Id,
                ["delegatorUserId"] = assignment.DelegatorUserId,
                ["delegateUserId"] = assignment.DelegateUserId,
                ["isActive"] = assignment.IsActive,
                ["reason"] = request.Reason?.Trim()
            }
        }, cancellationToken);

        return assignment.ToDetailDto();
    }
}
