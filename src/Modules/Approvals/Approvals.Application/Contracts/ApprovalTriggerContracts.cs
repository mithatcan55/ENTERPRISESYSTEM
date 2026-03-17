namespace Approvals.Application.Contracts;

/// <summary>
/// Business moduller approval motoruna bu ortak sozlesme ile baglanir.
/// Boylece her modul kendi icinde workflow resolve/start tekrar yazmaz.
/// </summary>
public sealed record ApprovalTriggerRequest(
    string ModuleKey,
    string DocumentType,
    string ReferenceType,
    string ReferenceId,
    int? RequesterUserId,
    string PayloadJson,
    bool RequireConfiguredWorkflow = false);

public sealed record ApprovalTriggerResult(
    bool RequiresApproval,
    bool Started,
    string Outcome,
    string? WorkflowCode,
    int? ApprovalInstanceId,
    string Message,
    ApprovalInstanceDetailDto? ApprovalInstance);
