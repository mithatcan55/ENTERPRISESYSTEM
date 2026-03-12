using Application.Exceptions;
using Identity.Application.Contracts;
using Identity.Application.Users.Commands;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Users.Commands;

public sealed class UpdateUserCommandHandler(IdentityDbContext identityDbContext) : IUpdateUserCommandHandler
{
    public async Task<UserListItemDto> HandleAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (userId <= 0)
        {
            throw new ValidationAppException(
                "Kullanici guncelleme dogrulamasi basarisiz.",
                new Dictionary<string, string[]>
                {
                    ["userId"] = ["Gecerli bir userId zorunludur."]
                });
        }

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ValidationAppException(
                "Kullanici guncelleme dogrulamasi basarisiz.",
                new Dictionary<string, string[]>
                {
                    ["username"] = ["Username zorunludur."],
                    ["email"] = ["Email zorunludur."]
                });
        }

        var user = await identityDbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);
        if (user is null)
            throw new NotFoundAppException($"Kullanici bulunamadi. userId={userId}");

        var normalizedUsername = request.Username.Trim();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var duplicateExists = await identityDbContext.Users
            .AsNoTracking()
            .AnyAsync(x => !x.IsDeleted && x.Id != userId && (x.Username == normalizedUsername || x.Email == normalizedEmail), cancellationToken);

        if (duplicateExists)
        {
            throw new ValidationAppException(
                "Kullanici benzersizlik kontrolu basarisiz.",
                new Dictionary<string, string[]>
                {
                    ["user"] = ["Ayni username veya email ile kayit zaten mevcut."]
                });
        }

        user.Username = normalizedUsername;
        user.Email = normalizedEmail;
        user.IsActive = request.IsActive;

        await identityDbContext.SaveChangesAsync(cancellationToken);

        return await identityDbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new UserListItemDto(
                x.Id,
                x.UserCode,
                x.Username,
                x.Email,
                x.IsActive,
                x.MustChangePassword,
                x.PasswordExpiresAt,
                x.CreatedAt))
            .FirstAsync(cancellationToken);
    }
}
