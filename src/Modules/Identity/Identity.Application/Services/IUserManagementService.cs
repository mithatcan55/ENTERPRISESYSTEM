using Identity.Application.Contracts;

namespace Identity.Application.Services;

public interface IUserManagementService
{
    Task<IReadOnlyList<UserListItemDto>> ListAsync(CancellationToken cancellationToken);
    Task<CreatedUserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<UserListItemDto> UpdateAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken);
    Task DeactivateAsync(int userId, CancellationToken cancellationToken);
    Task ReactivateAsync(int userId, CancellationToken cancellationToken);
    Task DeleteAsync(int userId, CancellationToken cancellationToken);
}
