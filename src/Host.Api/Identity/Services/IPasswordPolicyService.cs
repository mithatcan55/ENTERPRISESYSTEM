using Infrastructure.Persistence.Entities.Identity;

namespace Host.Api.Identity.Services;

public interface IPasswordPolicyService
{
    void ValidateComplexityOrThrow(string password, string? username, string? email);
    Task EnsureNotRecentlyUsedOrThrowAsync(int userId, string candidatePassword, CancellationToken cancellationToken);
    Task EnsureMinimumPasswordAgeOrThrowAsync(int userId, CancellationToken cancellationToken);
    Task RecordPasswordHistoryAsync(int userId, string passwordHash, CancellationToken cancellationToken);
    Task EnforcePasswordChangePolicyOrThrowAsync(User user, string candidatePassword, CancellationToken cancellationToken);
}
