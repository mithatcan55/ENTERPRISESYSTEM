using Host.Api.Identity.Contracts;

namespace Host.Api.Identity.Services;

public interface IUserManagementService
{
    Task<IReadOnlyList<UserListItemDto>> ListAsync(CancellationToken cancellationToken);
    Task<CreatedUserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken);
}
