using Approvals.Application.Contracts;

namespace Approvals.Application.Commands;

public interface IUpdateApprovalWorkflowCommandHandler
{
    Task<ApprovalWorkflowDetailDto> HandleAsync(int approvalWorkflowDefinitionId, UpdateApprovalWorkflowRequest request, CancellationToken cancellationToken);
}
