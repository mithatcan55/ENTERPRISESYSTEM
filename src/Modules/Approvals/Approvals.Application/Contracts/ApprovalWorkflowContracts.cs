namespace Approvals.Application.Contracts;

public sealed record ApprovalWorkflowQueryRequest(string? Search = null, bool? IsActive = null, int Page = 1, int PageSize = 20);

public sealed record ApprovalWorkflowStepDto(
    int Id,
    int StepOrder,
    string Name,
    string ApproverType,
    string ApproverValue,
    bool IsRequired,
    bool IsParallel,
    int MinimumApproverCount,
    int? DecisionDeadlineHours,
    string TimeoutDecision);

public sealed record ApprovalWorkflowConditionDto(
    int Id,
    string FieldKey,
    string Operator,
    string Value);

public sealed record ApprovalWorkflowListItemDto(
    int Id,
    string Code,
    string Name,
    string Description,
    string ModuleKey,
    string DocumentType,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? ModifiedAt);

public sealed record ApprovalWorkflowDetailDto(
    int Id,
    string Code,
    string Name,
    string Description,
    string ModuleKey,
    string DocumentType,
    bool IsActive,
    IReadOnlyList<ApprovalWorkflowStepDto> Steps,
    IReadOnlyList<ApprovalWorkflowConditionDto> Conditions);

public sealed record ApprovalWorkflowStepRequest(
    int StepOrder,
    string Name,
    string ApproverType,
    string ApproverValue,
    bool IsRequired,
    bool IsParallel,
    int MinimumApproverCount,
    int? DecisionDeadlineHours,
    string TimeoutDecision);

public sealed record ApprovalWorkflowConditionRequest(
    string FieldKey,
    string Operator,
    string Value);

public sealed record CreateApprovalWorkflowRequest(
    string Code,
    string Name,
    string Description,
    string ModuleKey,
    string DocumentType,
    bool IsActive,
    IReadOnlyList<ApprovalWorkflowStepRequest> Steps,
    IReadOnlyList<ApprovalWorkflowConditionRequest> Conditions);

public sealed record UpdateApprovalWorkflowRequest(
    string Name,
    string Description,
    string ModuleKey,
    string DocumentType,
    bool IsActive,
    IReadOnlyList<ApprovalWorkflowStepRequest> Steps,
    IReadOnlyList<ApprovalWorkflowConditionRequest> Conditions);

public sealed record ResolveApprovalWorkflowRequest(
    string ModuleKey,
    string DocumentType,
    string PayloadJson);

public sealed record ResolvedApprovalWorkflowStepDto(
    int StepOrder,
    string Name,
    string ApproverType,
    string ApproverValue,
    bool IsRequired,
    bool IsParallel,
    int MinimumApproverCount,
    int? DecisionDeadlineHours,
    string TimeoutDecision);

public sealed record ResolvedApprovalWorkflowDto(
    int WorkflowId,
    string Code,
    string Name,
    string ModuleKey,
    string DocumentType,
    int MatchedConditionCount,
    IReadOnlyList<ResolvedApprovalWorkflowStepDto> Steps);

public sealed record StartApprovalInstanceRequest(
    string ModuleKey,
    string DocumentType,
    string ReferenceType,
    string ReferenceId,
    int? RequesterUserId,
    string PayloadJson);

public sealed record ApprovalInstanceStepDto(
    int Id,
    int StepOrder,
    int? AssignedUserId,
    string Status,
    DateTime? DueAt);

public sealed record ApprovalDecisionDto(
    int Id,
    int ActorUserId,
    bool IsSystemDecision,
    string Decision,
    string Comment,
    DateTime CreatedAt);

public sealed record ApprovalInstanceDetailDto(
    int Id,
    int ApprovalWorkflowDefinitionId,
    string WorkflowCode,
    string ReferenceType,
    string ReferenceId,
    int RequesterUserId,
    string Status,
    int CurrentStepOrder,
    string PayloadJson,
    IReadOnlyList<ApprovalInstanceStepDto> Steps,
    IReadOnlyList<ApprovalDecisionDto> Decisions);

public sealed record PendingApprovalQueryRequest(int? AssignedUserId = null, string? Status = null, int Page = 1, int PageSize = 20);

public sealed record PendingApprovalListItemDto(
    int ApprovalInstanceId,
    int ApprovalInstanceStepId,
    string WorkflowCode,
    string ReferenceType,
    string ReferenceId,
    int StepOrder,
    int? AssignedUserId,
    string Status,
    DateTime CreatedAt);

public sealed record DecideApprovalStepRequest(
    string Decision,
    string Comment);
