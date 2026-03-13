using Approvals.Application.Contracts;

namespace Approvals.Application.Commands;

public interface ICreateDelegationAssignmentCommandHandler
{
    Task<DelegationAssignmentDetailDto> HandleAsync(CreateDelegationAssignmentRequest request, CancellationToken cancellationToken);
}
