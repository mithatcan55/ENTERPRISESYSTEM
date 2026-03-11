using Identity.Application.Contracts;

namespace Identity.Application.Roles.Commands;

public interface ICreateRoleCommandHandler
{
    Task<RoleListItemDto> HandleAsync(CreateRoleRequest request, CancellationToken cancellationToken);
}
