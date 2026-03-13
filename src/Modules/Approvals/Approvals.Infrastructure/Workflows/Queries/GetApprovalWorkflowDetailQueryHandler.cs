using Approvals.Application.Contracts;
using Approvals.Application.Queries;
using Application.Exceptions;
using global::Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Approvals.Infrastructure.Workflows.Queries;

public sealed class GetApprovalWorkflowDetailQueryHandler(ApprovalsDbContext dbContext) : IGetApprovalWorkflowDetailQueryHandler
{
    public async Task<ApprovalWorkflowDetailDto> HandleAsync(int approvalWorkflowDefinitionId, CancellationToken cancellationToken)
    {
        var workflow = await dbContext.ApprovalWorkflowDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == approvalWorkflowDefinitionId, cancellationToken)
            ?? throw new NotFoundAppException($"Approval workflow bulunamadi. Id={approvalWorkflowDefinitionId}");

        var steps = await dbContext.ApprovalWorkflowSteps
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.ApprovalWorkflowDefinitionId == workflow.Id)
            .OrderBy(x => x.StepOrder)
            .Select(x => x.ToDto())
            .ToListAsync(cancellationToken);

        var conditions = await dbContext.ApprovalWorkflowConditions
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.ApprovalWorkflowDefinitionId == workflow.Id)
            .OrderBy(x => x.FieldKey)
            .Select(x => x.ToDto())
            .ToListAsync(cancellationToken);

        return workflow.ToDetailDto(steps, conditions);
    }
}
