using Approvals.Application.Contracts;
using global::Infrastructure.Persistence.Entities.Approvals;

namespace Approvals.Infrastructure.Workflows;

internal static class ApprovalWorkflowMappingExtensions
{
    public static ApprovalWorkflowListItemDto ToListItemDto(this ApprovalWorkflowDefinition entity)
        => new(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.Description,
            entity.ModuleKey,
            entity.DocumentType,
            entity.IsActive,
            entity.CreatedAt,
            entity.ModifiedAt);

    public static ApprovalWorkflowDetailDto ToDetailDto(
        this ApprovalWorkflowDefinition entity,
        IReadOnlyList<ApprovalWorkflowStepDto> steps,
        IReadOnlyList<ApprovalWorkflowConditionDto> conditions)
        => new(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.Description,
            entity.ModuleKey,
            entity.DocumentType,
            entity.IsActive,
            steps,
            conditions);

    public static ApprovalWorkflowStepDto ToDto(this ApprovalWorkflowStep entity)
        => new(
            entity.Id,
            entity.StepOrder,
            entity.Name,
            entity.ApproverType,
            entity.ApproverValue,
            entity.IsRequired,
            entity.IsParallel,
            entity.MinimumApproverCount);

    public static ApprovalWorkflowConditionDto ToDto(this ApprovalWorkflowCondition entity)
        => new(entity.Id, entity.FieldKey, entity.Operator, entity.Value);
}
