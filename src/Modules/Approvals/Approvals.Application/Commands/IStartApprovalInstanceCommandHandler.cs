using Approvals.Application.Contracts;

namespace Approvals.Application.Commands;

public interface IStartApprovalInstanceCommandHandler
{
    Task<ApprovalInstanceDetailDto> HandleAsync(StartApprovalInstanceRequest request, CancellationToken cancellationToken);
}
