using Approvals.Application.Commands;
using Approvals.Application.Contracts;
using Application.Exceptions;
using global::Infrastructure.Persistence;
using global::Infrastructure.Persistence.Entities.Approvals;
using Microsoft.EntityFrameworkCore;

namespace Approvals.Infrastructure.Workflows.Commands;

public sealed class CreateApprovalWorkflowCommandHandler(ApprovalsDbContext dbContext) : ICreateApprovalWorkflowCommandHandler
{
    public async Task<ApprovalWorkflowDetailDto> HandleAsync(CreateApprovalWorkflowRequest request, CancellationToken cancellationToken)
    {
        ValidateRequest(request.Code, request.Name, request.ModuleKey, request.DocumentType, request.Steps);

        if (await dbContext.ApprovalWorkflowDefinitions.AnyAsync(x => !x.IsDeleted && x.Code == request.Code, cancellationToken))
        {
            throw new ValidationAppException("Ayni kodla baska bir workflow zaten var.", new Dictionary<string, string[]>
            {
                ["code"] = ["Workflow code benzersiz olmalidir."]
            });
        }

        var workflow = new ApprovalWorkflowDefinition
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            ModuleKey = request.ModuleKey.Trim(),
            DocumentType = request.DocumentType.Trim(),
            IsActive = request.IsActive
        };

        dbContext.ApprovalWorkflowDefinitions.Add(workflow);
        await dbContext.SaveChangesAsync(cancellationToken);

        await ReplaceChildCollectionsAsync(workflow.Id, request.Steps, request.Conditions, cancellationToken);

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

    private static void ValidateRequest(string code, string name, string moduleKey, string documentType, IReadOnlyList<ApprovalWorkflowStepRequest> steps)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(code)) errors["code"] = ["Workflow code zorunludur."];
        if (string.IsNullOrWhiteSpace(name)) errors["name"] = ["Workflow adi zorunludur."];
        if (string.IsNullOrWhiteSpace(moduleKey)) errors["moduleKey"] = ["Module key zorunludur."];
        if (string.IsNullOrWhiteSpace(documentType)) errors["documentType"] = ["Document type zorunludur."];
        if (steps.Count == 0) errors["steps"] = ["En az bir onay adimi tanimlanmalidir."];

        if (errors.Count > 0)
        {
            throw new ValidationAppException("Approval workflow request gecersiz.", errors);
        }
    }

    private async Task ReplaceChildCollectionsAsync(
        int workflowId,
        IReadOnlyList<ApprovalWorkflowStepRequest> stepRequests,
        IReadOnlyList<ApprovalWorkflowConditionRequest> conditionRequests,
        CancellationToken cancellationToken)
    {
        foreach (var step in stepRequests.OrderBy(x => x.StepOrder))
        {
            dbContext.ApprovalWorkflowSteps.Add(new ApprovalWorkflowStep
            {
                ApprovalWorkflowDefinitionId = workflowId,
                StepOrder = step.StepOrder,
                Name = step.Name.Trim(),
                ApproverType = step.ApproverType.Trim(),
                ApproverValue = step.ApproverValue.Trim(),
                IsRequired = step.IsRequired,
                IsParallel = step.IsParallel,
                MinimumApproverCount = Math.Max(step.MinimumApproverCount, 1)
            });
        }

        foreach (var condition in conditionRequests)
        {
            dbContext.ApprovalWorkflowConditions.Add(new ApprovalWorkflowCondition
            {
                ApprovalWorkflowDefinitionId = workflowId,
                FieldKey = condition.FieldKey.Trim(),
                Operator = condition.Operator.Trim(),
                Value = condition.Value.Trim()
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
