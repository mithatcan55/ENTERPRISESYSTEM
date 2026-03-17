using Approvals.Application.Contracts;

namespace Approvals.Application.Commands;

public interface ISetDelegationAssignmentStatusCommandHandler
{
    Task<DelegationAssignmentDetailDto> HandleAsync(int delegationAssignmentId, SetDelegationAssignmentStatusRequest request, CancellationToken cancellationToken);
}
