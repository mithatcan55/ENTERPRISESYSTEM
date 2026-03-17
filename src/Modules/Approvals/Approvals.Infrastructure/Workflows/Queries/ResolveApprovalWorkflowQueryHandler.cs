using Approvals.Application.Contracts;
using Approvals.Application.Queries;

namespace Approvals.Infrastructure.Workflows.Queries;

public sealed class ResolveApprovalWorkflowQueryHandler(ApprovalWorkflowResolver workflowResolver) : IResolveApprovalWorkflowQueryHandler
{
    public async Task<ResolvedApprovalWorkflowDto> HandleAsync(ResolveApprovalWorkflowRequest request, CancellationToken cancellationToken)
    {
        var resolved = await workflowResolver.ResolveAsync(request.ModuleKey, request.DocumentType, request.PayloadJson, cancellationToken);

        return new ResolvedApprovalWorkflowDto(
            resolved.Workflow.Id,
            resolved.Workflow.Code,
            resolved.Workflow.Name,
            resolved.Workflow.ModuleKey,
            resolved.Workflow.DocumentType,
            resolved.MatchedConditionCount,
            resolved.Steps.Select(step => new ResolvedApprovalWorkflowStepDto(
                step.StepOrder,
                step.Name,
                step.ApproverType,
                step.ApproverValue,
                step.IsRequired,
                step.IsParallel,
                step.MinimumApproverCount,
                step.DecisionDeadlineHours,
                step.TimeoutDecision)).ToList());
    }
}
