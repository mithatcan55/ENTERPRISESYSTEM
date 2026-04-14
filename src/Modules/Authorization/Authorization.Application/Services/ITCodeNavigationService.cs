using Authorization.Application.Contracts;

namespace Authorization.Application.Services;

public interface ITCodeNavigationService
{
    Task<IReadOnlyList<TCodeNavigationItemDto>> SearchAsync(
        int userId,
        string? query,
        int take,
        CancellationToken cancellationToken);

    Task<TCodeNavigationItemDto?> ResolveAsync(
        int userId,
        string transactionCode,
        CancellationToken cancellationToken);
}
