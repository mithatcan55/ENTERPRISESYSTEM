using Approvals.Application.Contracts;
using global::Infrastructure.Persistence.Entities.Approvals;

namespace Approvals.Infrastructure.Workflows;

internal static class ApprovalRuntimeMappingExtensions
{
    public static ApprovalInstanceStepDto ToDto(this ApprovalInstanceStep entity)
        => new(entity.Id, entity.StepOrder, entity.AssignedUserId, entity.Status, entity.DueAt);

    public static ApprovalDecisionDto ToDto(this ApprovalDecision entity)
        => new(entity.Id, entity.ActorUserId, entity.IsSystemDecision, entity.Decision, entity.Comment, entity.CreatedAt);
}
