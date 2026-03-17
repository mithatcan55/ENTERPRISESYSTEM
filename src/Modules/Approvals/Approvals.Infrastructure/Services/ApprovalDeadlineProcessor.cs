using Application.Observability;
using global::Infrastructure.Persistence;
using global::Infrastructure.Persistence.Entities.Approvals;
using Microsoft.EntityFrameworkCore;

namespace Approvals.Infrastructure.Services;

/// <summary>
/// Deadline dolan pending approval step'leri sistem karari ile kapatir.
/// Worker bu sinifi periyodik cagirir; testler ise dogrudan tek batch isletir.
/// </summary>
public sealed class ApprovalDeadlineProcessor(
    ApprovalsDbContext approvalsDbContext,
    IOperationalEventPublisher operationalEventPublisher)
{
    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var expiredSteps = await approvalsDbContext.ApprovalInstanceSteps
            .Where(x => !x.IsDeleted && x.Status == "Pending" && x.DueAt.HasValue && x.DueAt <= now)
            .OrderBy(x => x.DueAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (expiredSteps.Count == 0)
        {
            return 0;
        }

        var workflowStepIds = expiredSteps.Select(x => x.ApprovalWorkflowStepId).Distinct().ToList();
        var workflowSteps = await approvalsDbContext.ApprovalWorkflowSteps
            .AsNoTracking()
            .Where(x => !x.IsDeleted && workflowStepIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var instanceIds = expiredSteps.Select(x => x.ApprovalInstanceId).Distinct().ToList();
        var instances = await approvalsDbContext.ApprovalInstances
            .Where(x => !x.IsDeleted && instanceIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var processedCount = 0;

        foreach (var expiredStep in expiredSteps)
        {
            if (!instances.TryGetValue(expiredStep.ApprovalInstanceId, out var instance)
                || !workflowSteps.TryGetValue(expiredStep.ApprovalWorkflowStepId, out var workflowStep))
            {
                continue;
            }

            if (expiredStep.Status != "Pending" || instance.Status != "Pending")
            {
                continue;
            }

            var timeoutDecision = string.IsNullOrWhiteSpace(workflowStep.TimeoutDecision)
                ? "reject"
                : workflowStep.TimeoutDecision.Trim().ToLowerInvariant();

            approvalsDbContext.ApprovalDecisions.Add(new ApprovalDecision
            {
                ApprovalInstanceStepId = expiredStep.Id,
                ActorUserId = 0,
                IsSystemDecision = true,
                Decision = timeoutDecision,
                Comment = $"Sistem karari: step deadline doldu ({expiredStep.DueAt:O})."
            });

            expiredStep.Status = timeoutDecision == "approve" ? "Approved" : "Rejected";

            if (timeoutDecision == "approve")
            {
                var siblingSteps = await approvalsDbContext.ApprovalInstanceSteps
                    .Where(x => !x.IsDeleted && x.ApprovalInstanceId == instance.Id && x.StepOrder == expiredStep.StepOrder)
                    .OrderBy(x => x.Id)
                    .ToListAsync(cancellationToken);

                foreach (var sibling in siblingSteps.Where(x => x.Status == "Pending" && x.Id != expiredStep.Id))
                {
                    sibling.Status = "Skipped";
                }

                var nextStepOrder = await approvalsDbContext.ApprovalInstanceSteps
                    .Where(x => !x.IsDeleted && x.ApprovalInstanceId == instance.Id && x.StepOrder > expiredStep.StepOrder)
                    .OrderBy(x => x.StepOrder)
                    .Select(x => (int?)x.StepOrder)
                    .FirstOrDefaultAsync(cancellationToken);

                if (nextStepOrder.HasValue)
                {
                    var nextSteps = await approvalsDbContext.ApprovalInstanceSteps
                        .Where(x => !x.IsDeleted && x.ApprovalInstanceId == instance.Id && x.StepOrder == nextStepOrder.Value)
                        .ToListAsync(cancellationToken);

                    var nextWorkflowStepIds = nextSteps.Select(x => x.ApprovalWorkflowStepId).Distinct().ToList();
                    var nextWorkflowSteps = await approvalsDbContext.ApprovalWorkflowSteps
                        .AsNoTracking()
                        .Where(x => !x.IsDeleted && nextWorkflowStepIds.Contains(x.Id))
                        .ToDictionaryAsync(x => x.Id, cancellationToken);

                    foreach (var nextStep in nextSteps.Where(x => x.Status == "Waiting"))
                    {
                        nextStep.Status = "Pending";
                        nextStep.DueAt = nextWorkflowSteps.TryGetValue(nextStep.ApprovalWorkflowStepId, out var nextWorkflowStep)
                            && nextWorkflowStep.DecisionDeadlineHours.HasValue
                            ? DateTime.UtcNow.AddHours(nextWorkflowStep.DecisionDeadlineHours.Value)
                            : null;
                    }

                    instance.CurrentStepOrder = nextStepOrder.Value;
                    instance.Status = "Pending";
                }
                else
                {
                    instance.CurrentStepOrder = expiredStep.StepOrder;
                    instance.Status = "Approved";
                }
            }
            else
            {
                instance.CurrentStepOrder = expiredStep.StepOrder;
                instance.Status = "Rejected";
            }

            await approvalsDbContext.SaveChangesAsync(cancellationToken);

            await operationalEventPublisher.PublishAsync(new OperationalEvent
            {
                EventName = "approval.step.deadline.processed",
                Category = "ApprovalWorkflow",
                Severity = "Warning",
                Action = "SystemDecision",
                Resource = "ApprovalInstanceStep",
                Message = $"Approval step deadline nedeniyle sistem karari uygulandi. StepId={expiredStep.Id}",
                UserId = "system",
                IsSuccessful = true,
                Properties =
                {
                    ["approvalInstanceId"] = instance.Id,
                    ["approvalInstanceStepId"] = expiredStep.Id,
                    ["workflowStepId"] = workflowStep.Id,
                    ["timeoutDecision"] = timeoutDecision,
                    ["dueAt"] = expiredStep.DueAt
                }
            }, cancellationToken);

            processedCount += 1;
        }

        return processedCount;
    }
}
