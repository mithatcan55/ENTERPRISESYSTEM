using Approvals.Application.Commands;
using Approvals.Application.Contracts;
using Approvals.Application.Queries;
using Approvals.Application.Services;
using Approvals.Infrastructure.Workflows;
using Application.Exceptions;
using global::Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Approvals.Infrastructure.Services;

/// <summary>
/// Business modul sadece referans ve payload verir.
/// Bu servis workflow resolve, mevcut pending instance kontrolu ve gerekiyorsa yeni instance acma isini ustlenir.
/// </summary>
public sealed class ApprovalTriggerService(
    ApprovalsDbContext approvalsDbContext,
    ApprovalWorkflowResolver approvalWorkflowResolver,
    IStartApprovalInstanceCommandHandler startApprovalInstanceCommandHandler,
    IGetApprovalInstanceDetailQueryHandler getApprovalInstanceDetailQueryHandler) : IApprovalTriggerService
{
    public async Task<ApprovalTriggerResult> TriggerAsync(ApprovalTriggerRequest request, CancellationToken cancellationToken)
    {
        ResolvedWorkflowSelection? resolvedSelection;

        try
        {
            resolvedSelection = await approvalWorkflowResolver.ResolveAsync(
                request.ModuleKey,
                request.DocumentType,
                request.PayloadJson,
                cancellationToken);
        }
        catch (NotFoundAppException) when (!request.RequireConfiguredWorkflow)
        {
            return new ApprovalTriggerResult(
                RequiresApproval: false,
                Started: false,
                Outcome: "not_required",
                WorkflowCode: null,
                ApprovalInstanceId: null,
                Message: "Bu islem icin eslesen approval workflow bulunamadi.",
                ApprovalInstance: null);
        }

        var existingPendingInstanceId = await approvalsDbContext.ApprovalInstances
            .AsNoTracking()
            .Where(x => !x.IsDeleted
                        && x.ReferenceType == request.ReferenceType
                        && x.ReferenceId == request.ReferenceId
                        && x.Status == "Pending")
            .OrderByDescending(x => x.Id)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingPendingInstanceId.HasValue)
        {
            var existingInstance = await getApprovalInstanceDetailQueryHandler.HandleAsync(existingPendingInstanceId.Value, cancellationToken);

            return new ApprovalTriggerResult(
                RequiresApproval: true,
                Started: false,
                Outcome: "already_pending",
                WorkflowCode: resolvedSelection.Workflow.Code,
                ApprovalInstanceId: existingInstance.Id,
                Message: "Bu referans icin zaten bekleyen approval instance mevcut.",
                ApprovalInstance: existingInstance);
        }

        var startedInstance = await startApprovalInstanceCommandHandler.HandleAsync(
            new StartApprovalInstanceRequest(
                request.ModuleKey,
                request.DocumentType,
                request.ReferenceType,
                request.ReferenceId,
                request.RequesterUserId,
                request.PayloadJson),
            cancellationToken);

        return new ApprovalTriggerResult(
            RequiresApproval: true,
            Started: true,
            Outcome: "started",
            WorkflowCode: resolvedSelection.Workflow.Code,
            ApprovalInstanceId: startedInstance.Id,
            Message: "Approval instance baslatildi.",
            ApprovalInstance: startedInstance);
    }
}
