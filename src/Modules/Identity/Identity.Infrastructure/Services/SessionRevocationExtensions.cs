using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Services;

internal static class SessionRevocationExtensions
{
    public static async Task RevokeAllSessionsAndRefreshTokensAsync(
        this IdentityDbContext identityDbContext,
        int userId,
        string revokedBy,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var sessions = await identityDbContext.UserSessions
            .Where(x => x.UserId == userId && !x.IsDeleted && !x.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            session.IsRevoked = true;
            session.RevokedAt = now;
            session.RevokedBy = revokedBy;
        }

        var refreshTokens = await identityDbContext.UserRefreshTokens
            .Where(x => x.UserId == userId && !x.IsDeleted && !x.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = now;
            refreshToken.RevokedBy = revokedBy;
        }

        await identityDbContext.SaveChangesAsync(cancellationToken);
    }
}
