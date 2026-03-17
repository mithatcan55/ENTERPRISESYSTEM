using Approvals.Application.Contracts;

namespace Approvals.Application.Queries;

public interface IResolveApprovalWorkflowQueryHandler
{
    Task<ResolvedApprovalWorkflowDto> HandleAsync(ResolveApprovalWorkflowRequest request, CancellationToken cancellationToken);
}
