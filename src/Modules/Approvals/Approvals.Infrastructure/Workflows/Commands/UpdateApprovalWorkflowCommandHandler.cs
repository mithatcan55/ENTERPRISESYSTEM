using Approvals.Application.Commands;
using Approvals.Application.Contracts;
using Application.Exceptions;
using global::Infrastructure.Persistence;
using global::Infrastructure.Persistence.Entities.Approvals;
using Microsoft.EntityFrameworkCore;

namespace Approvals.Infrastructure.Workflows.Commands;

public sealed class UpdateApprovalWorkflowCommandHandler(ApprovalsDbContext dbContext) : IUpdateApprovalWorkflowCommandHandler
{
    public async Task<ApprovalWorkflowDetailDto> HandleAsync(int approvalWorkflowDefinitionId, UpdateApprovalWorkflowRequest request, CancellationToken cancellationToken)
    {
        var workflow = await dbContext.ApprovalWorkflowDefinitions
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == approvalWorkflowDefinitionId, cancellationToken)
            ?? throw new NotFoundAppException($"Approval workflow bulunamadi. Id={approvalWorkflowDefinitionId}");

        if (request.Steps.Count == 0)
        {
            throw new ValidationAppException("Workflow guncelleme istegi gecersiz.", new Dictionary<string, string[]>
            {
                ["steps"] = ["En az bir onay adimi tanimlanmalidir."]
            });
        }

        workflow.Name = request.Name.Trim();
        workflow.Description = request.Description.Trim();
        workflow.ModuleKey = request.ModuleKey.Trim();
        workflow.DocumentType = request.DocumentType.Trim();
        workflow.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);

        var currentSteps = await dbContext.ApprovalWorkflowSteps
            .Where(x => !x.IsDeleted && x.ApprovalWorkflowDefinitionId == workflow.Id)
            .ToListAsync(cancellationToken);

        var currentConditions = await dbContext.ApprovalWorkflowConditions
            .Where(x => !x.IsDeleted && x.ApprovalWorkflowDefinitionId == workflow.Id)
            .ToListAsync(cancellationToken);

        foreach (var step in currentSteps)
        {
            step.IsDeleted = true;
            step.DeletedAt = DateTime.UtcNow;
        }

        foreach (var condition in currentConditions)
        {
            condition.IsDeleted = true;
            condition.DeletedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var step in request.Steps.OrderBy(x => x.StepOrder))
        {
            dbContext.ApprovalWorkflowSteps.Add(new ApprovalWorkflowStep
            {
                ApprovalWorkflowDefinitionId = workflow.Id,
                StepOrder = step.StepOrder,
                Name = step.Name.Trim(),
                ApproverType = step.ApproverType.Trim(),
                ApproverValue = step.ApproverValue.Trim(),
                IsRequired = step.IsRequired,
                IsParallel = step.IsParallel,
                MinimumApproverCount = Math.Max(step.MinimumApproverCount, 1)
            });
        }

        foreach (var condition in request.Conditions)
        {
            dbContext.ApprovalWorkflowConditions.Add(new ApprovalWorkflowCondition
            {
                ApprovalWorkflowDefinitionId = workflow.Id,
                FieldKey = condition.FieldKey.Trim(),
                Operator = condition.Operator.Trim(),
                Value = condition.Value.Trim()
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var steps = await dbContext.ApprovalWorkflowSteps.AsNoTracking()
            .Where(x => !x.IsDeleted && x.ApprovalWorkflowDefinitionId == workflow.Id)
            .OrderBy(x => x.StepOrder)
            .Select(x => x.ToDto())
            .ToListAsync(cancellationToken);

        var conditions = await dbContext.ApprovalWorkflowConditions.AsNoTracking()
            .Where(x => !x.IsDeleted && x.ApprovalWorkflowDefinitionId == workflow.Id)
            .OrderBy(x => x.FieldKey)
            .Select(x => x.ToDto())
            .ToListAsync(cancellationToken);

        return workflow.ToDetailDto(steps, conditions);
    }
}
