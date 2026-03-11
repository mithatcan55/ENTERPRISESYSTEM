using Identity.Application.Contracts;

namespace Identity.Application.Services;

public interface IAuthLifecycleService
{
    Task<LoginResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<RefreshTokenResponseDto> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
    Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<SessionListItemDto>> ListSessionsAsync(int userId, bool onlyActive, CancellationToken cancellationToken);
    Task RevokeSessionAsync(int sessionId, string? reason, CancellationToken cancellationToken);
}
