using Approvals.Application.Contracts;
using Approvals.Application.Queries;
using Application.Exceptions;
using global::Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Approvals.Infrastructure.Workflows.Queries;

public sealed class GetApprovalInstanceDetailQueryHandler(
    ApprovalsDbContext approvalsDbContext) : IGetApprovalInstanceDetailQueryHandler
{
    public async Task<ApprovalInstanceDetailDto> HandleAsync(int approvalInstanceId, CancellationToken cancellationToken)
    {
        var instance = await approvalsDbContext.ApprovalInstances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == approvalInstanceId, cancellationToken)
            ?? throw new NotFoundAppException($"Approval instance bulunamadi. Id={approvalInstanceId}");

        var workflowCode = await approvalsDbContext.ApprovalWorkflowDefinitions
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Id == instance.ApprovalWorkflowDefinitionId)
            .Select(x => x.Code)
            .FirstAsync(cancellationToken);

        var steps = await approvalsDbContext.ApprovalInstanceSteps
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.ApprovalInstanceId == instance.Id)
            .OrderBy(x => x.StepOrder)
            .ThenBy(x => x.Id)
            .Select(x => x.ToDto())
            .ToListAsync(cancellationToken);

        var decisions = await approvalsDbContext.ApprovalDecisions
            .AsNoTracking()
            .Join(
                approvalsDbContext.ApprovalInstanceSteps.AsNoTracking(),
                decision => decision.ApprovalInstanceStepId,
                step => step.Id,
                (decision, step) => new { decision, step })
            .Where(x => !x.decision.IsDeleted && !x.step.IsDeleted && x.step.ApprovalInstanceId == instance.Id)
            .OrderBy(x => x.decision.CreatedAt)
            .Select(x => x.decision.ToDto())
            .ToListAsync(cancellationToken);

        return new ApprovalInstanceDetailDto(
            instance.Id,
            instance.ApprovalWorkflowDefinitionId,
            workflowCode,
            instance.ReferenceType,
            instance.ReferenceId,
            instance.RequesterUserId,
            instance.Status,
            instance.CurrentStepOrder,
            instance.PayloadJson,
            steps,
            decisions);
    }
}
