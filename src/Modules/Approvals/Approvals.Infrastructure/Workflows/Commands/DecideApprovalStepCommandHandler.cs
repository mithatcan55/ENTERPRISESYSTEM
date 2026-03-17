using Approvals.Application.Commands;
using Approvals.Application.Contracts;
using Approvals.Application.Queries;
using Application.Exceptions;
using Application.Security;
using global::Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Approvals.Infrastructure.Workflows.Commands;

public sealed class DecideApprovalStepCommandHandler(
    ApprovalsDbContext approvalsDbContext,
    ICurrentUserContext currentUserContext,
    IGetApprovalInstanceDetailQueryHandler getApprovalInstanceDetailQueryHandler) : IDecideApprovalStepCommandHandler
{
    public async Task<ApprovalInstanceDetailDto> HandleAsync(int approvalInstanceStepId, DecideApprovalStepRequest request, CancellationToken cancellationToken)
    {
        if (!currentUserContext.TryGetUserId(out var actorUserId))
        {
            throw new ForbiddenAppException("Approval karari icin kullanici kimligi bulunamadi.");
        }

        var normalizedDecision = request.Decision.Trim().ToLowerInvariant();
        if (normalizedDecision is not ("approve" or "reject" or "return"))
        {
            throw new ValidationAppException("Desteklenmeyen approval karari.", new Dictionary<string, string[]>
            {
                ["decision"] = ["Decision sadece approve, reject veya return olabilir."]
            });
        }

        var step = await approvalsDbContext.ApprovalInstanceSteps
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == approvalInstanceStepId, cancellationToken)
            ?? throw new NotFoundAppException($"Approval step bulunamadi. Id={approvalInstanceStepId}");

        if (step.AssignedUserId != actorUserId && !currentUserContext.IsInRole("SYS_ADMIN"))
        {
            throw new ForbiddenAppException("Bu approval step kullaniciya atanmamis.");
        }

        if (step.Status != "Pending")
        {
            throw new ValidationAppException("Sadece pending durumundaki approval step karara baglanabilir.", new Dictionary<string, string[]>
            {
                ["status"] = [$"Mevcut durum uygun degil: {step.Status}"]
            });
        }

        var instance = await approvalsDbContext.ApprovalInstances
            .FirstAsync(x => !x.IsDeleted && x.Id == step.ApprovalInstanceId, cancellationToken);

        var workflowStep = await approvalsDbContext.ApprovalWorkflowSteps
            .FirstAsync(x => !x.IsDeleted && x.Id == step.ApprovalWorkflowStepId, cancellationToken);

        approvalsDbContext.ApprovalDecisions.Add(new global::Infrastructure.Persistence.Entities.Approvals.ApprovalDecision
        {
            ApprovalInstanceStepId = step.Id,
            ActorUserId = actorUserId,
            IsSystemDecision = false,
            Decision = normalizedDecision,
            Comment = request.Comment?.Trim() ?? string.Empty
        });

        if (normalizedDecision == "reject")
        {
            step.Status = "Rejected";
            instance.Status = "Rejected";
            await approvalsDbContext.SaveChangesAsync(cancellationToken);
            return await getApprovalInstanceDetailQueryHandler.HandleAsync(instance.Id, cancellationToken);
        }

        if (normalizedDecision == "return")
        {
            step.Status = "Returned";
            instance.Status = "Returned";
            await approvalsDbContext.SaveChangesAsync(cancellationToken);
            return await getApprovalInstanceDetailQueryHandler.HandleAsync(instance.Id, cancellationToken);
        }

        step.Status = "Approved";

        var siblingSteps = await approvalsDbContext.ApprovalInstanceSteps
            .Where(x => !x.IsDeleted && x.ApprovalInstanceId == instance.Id && x.StepOrder == step.StepOrder)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var approvedCount = siblingSteps.Count(x => x.Status == "Approved") + (step.Status == "Approved" && siblingSteps.All(x => x.Id != step.Id) ? 1 : 0);
        if (approvedCount < workflowStep.MinimumApproverCount)
        {
            await approvalsDbContext.SaveChangesAsync(cancellationToken);
            return await getApprovalInstanceDetailQueryHandler.HandleAsync(instance.Id, cancellationToken);
        }

        foreach (var sibling in siblingSteps.Where(x => x.Status == "Pending" && x.Id != step.Id))
        {
            sibling.Status = "Skipped";
        }

        var nextStepOrder = await approvalsDbContext.ApprovalInstanceSteps
            .Where(x => !x.IsDeleted && x.ApprovalInstanceId == instance.Id && x.StepOrder > step.StepOrder)
            .OrderBy(x => x.StepOrder)
            .Select(x => (int?)x.StepOrder)
            .FirstOrDefaultAsync(cancellationToken);

        if (!nextStepOrder.HasValue)
        {
            instance.Status = "Approved";
            instance.CurrentStepOrder = step.StepOrder;
            await approvalsDbContext.SaveChangesAsync(cancellationToken);
            return await getApprovalInstanceDetailQueryHandler.HandleAsync(instance.Id, cancellationToken);
        }

        var nextSteps = await approvalsDbContext.ApprovalInstanceSteps
            .Where(x => !x.IsDeleted && x.ApprovalInstanceId == instance.Id && x.StepOrder == nextStepOrder.Value)
            .ToListAsync(cancellationToken);

        var workflowStepLookup = await approvalsDbContext.ApprovalWorkflowSteps
            .AsNoTracking()
            .Where(x => !x.IsDeleted && nextSteps.Select(stepItem => stepItem.ApprovalWorkflowStepId).Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        foreach (var nextStep in nextSteps)
        {
            if (nextStep.Status == "Waiting")
            {
                nextStep.Status = "Pending";
                nextStep.DueAt = workflowStepLookup.TryGetValue(nextStep.ApprovalWorkflowStepId, out var nextWorkflowStep)
                    && nextWorkflowStep.DecisionDeadlineHours.HasValue
                    ? DateTime.UtcNow.AddHours(nextWorkflowStep.DecisionDeadlineHours.Value)
                    : null;
            }
        }

        instance.CurrentStepOrder = nextStepOrder.Value;
        instance.Status = "Pending";
        await approvalsDbContext.SaveChangesAsync(cancellationToken);
        return await getApprovalInstanceDetailQueryHandler.HandleAsync(instance.Id, cancellationToken);
    }
}
