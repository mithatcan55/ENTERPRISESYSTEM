using Approvals.Application.Contracts;

namespace Approvals.Application.Commands;

public interface IDecideApprovalStepCommandHandler
{
    Task<ApprovalInstanceDetailDto> HandleAsync(int approvalInstanceStepId, DecideApprovalStepRequest request, CancellationToken cancellationToken);
}
