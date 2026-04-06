using Identity.Application.Contracts;

namespace Identity.Application.Users.Queries;

public interface IGetUserPermissionSummaryQueryHandler
{
    Task<UserPermissionSummaryDto> HandleAsync(int userId, CancellationToken cancellationToken);
}
