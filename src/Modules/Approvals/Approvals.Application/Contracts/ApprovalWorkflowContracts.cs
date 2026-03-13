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
    int MinimumApproverCount);

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
    int MinimumApproverCount);

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
