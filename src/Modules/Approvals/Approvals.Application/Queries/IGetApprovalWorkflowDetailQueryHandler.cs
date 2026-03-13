using Approvals.Application.Contracts;

namespace Approvals.Application.Queries;

public interface IGetApprovalWorkflowDetailQueryHandler
{
    Task<ApprovalWorkflowDetailDto> HandleAsync(int approvalWorkflowDefinitionId, CancellationToken cancellationToken);
}
