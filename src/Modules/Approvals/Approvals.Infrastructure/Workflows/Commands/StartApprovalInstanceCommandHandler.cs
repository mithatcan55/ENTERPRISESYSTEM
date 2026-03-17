using Approvals.Application.Commands;
using Approvals.Application.Contracts;
using Approvals.Application.Queries;
using Application.Exceptions;
using Application.Security;
using global::Infrastructure.Persistence;
using global::Infrastructure.Persistence.Entities.Approvals;
using Microsoft.EntityFrameworkCore;

namespace Approvals.Infrastructure.Workflows.Commands;

public sealed class StartApprovalInstanceCommandHandler(
    ApprovalsDbContext approvalsDbContext,
    ICurrentUserContext currentUserContext,
    ApprovalWorkflowResolver workflowResolver,
    ApproverResolutionService approverResolutionService,
    IGetApprovalInstanceDetailQueryHandler getApprovalInstanceDetailQueryHandler) : IStartApprovalInstanceCommandHandler
{
    public async Task<ApprovalInstanceDetailDto> HandleAsync(StartApprovalInstanceRequest request, CancellationToken cancellationToken)
    {
        // Requester user acik verilmediyse aktif claim set'inden cekeriz.
        // Boylece approval baslatma endpoint'i hem explicit hem implicit actor ile calisabilir.
        var requesterUserId = request.RequesterUserId;
        if (!requesterUserId.HasValue && currentUserContext.TryGetUserId(out var currentUserId))
        {
            requesterUserId = currentUserId;
        }

        if (!requesterUserId.HasValue)
        {
            throw new ValidationAppException("Requester user belirlenemedi.", new Dictionary<string, string[]>
            {
                ["requesterUserId"] = ["Approval instance baslatmak icin requester user gerekir."]
            });
        }

        if (await approvalsDbContext.ApprovalInstances.AnyAsync(
                x => !x.IsDeleted && x.ReferenceType == request.ReferenceType && x.ReferenceId == request.ReferenceId && x.Status == "Pending",
                cancellationToken))
        {
            throw new ValidationAppException("Ayni referans icin acik approval instance zaten var.", new Dictionary<string, string[]>
            {
                ["referenceId"] = ["Bu kayit icin bekleyen approval instance zaten mevcut."]
            });
        }

        var resolved = await workflowResolver.ResolveAsync(request.ModuleKey, request.DocumentType, request.PayloadJson, cancellationToken);
        var firstStepOrder = resolved.Steps.Min(x => x.StepOrder);

        // Runtime instance kaydi once acilir, step satirlari daha sonra buna baglanir.
        var instance = new ApprovalInstance
        {
            ApprovalWorkflowDefinitionId = resolved.Workflow.Id,
            ReferenceType = request.ReferenceType.Trim(),
            ReferenceId = request.ReferenceId.Trim(),
            RequesterUserId = requesterUserId.Value,
            Status = "Pending",
            CurrentStepOrder = firstStepOrder,
            PayloadJson = string.IsNullOrWhiteSpace(request.PayloadJson) ? "{}" : request.PayloadJson.Trim()
        };

        approvalsDbContext.ApprovalInstances.Add(instance);
        await approvalsDbContext.SaveChangesAsync(cancellationToken);

        foreach (var step in resolved.Steps)
        {
            // Approver tipi role veya specific_user olsa da sonuc bu noktada
            // somut atanmis user listesine donusmus olur.
            var assignedUserIds = await approverResolutionService.ResolveAssignedUserIdsAsync(
                resolved.Workflow.Code,
                step.ApproverType,
                step.ApproverValue,
                cancellationToken);

            foreach (var assignedUserId in assignedUserIds)
            {
                approvalsDbContext.ApprovalInstanceSteps.Add(new ApprovalInstanceStep
                {
                    ApprovalInstanceId = instance.Id,
                    ApprovalWorkflowStepId = step.Id,
                    StepOrder = step.StepOrder,
                    AssignedUserId = assignedUserId,
                    Status = step.StepOrder == firstStepOrder ? "Pending" : "Waiting",
                    DueAt = step.StepOrder == firstStepOrder && step.DecisionDeadlineHours.HasValue
                        ? DateTime.UtcNow.AddHours(step.DecisionDeadlineHours.Value)
                        : null
                });
            }
        }

        await approvalsDbContext.SaveChangesAsync(cancellationToken);
        return await getApprovalInstanceDetailQueryHandler.HandleAsync(instance.Id, cancellationToken);
    }
}
