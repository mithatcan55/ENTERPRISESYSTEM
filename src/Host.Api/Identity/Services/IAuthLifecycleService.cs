using Host.Api.Identity.Contracts;

namespace Host.Api.Identity.Services;

public interface IAuthLifecycleService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<SessionListItemDto>> ListSessionsAsync(int userId, bool onlyActive, CancellationToken cancellationToken);
    Task RevokeSessionAsync(int sessionId, string? reason, CancellationToken cancellationToken);
}
