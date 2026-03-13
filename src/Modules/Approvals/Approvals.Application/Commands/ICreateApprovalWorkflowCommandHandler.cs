using Approvals.Application.Contracts;

namespace Approvals.Application.Commands;

public interface ICreateApprovalWorkflowCommandHandler
{
    Task<ApprovalWorkflowDetailDto> HandleAsync(CreateApprovalWorkflowRequest request, CancellationToken cancellationToken);
}
