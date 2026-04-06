using System.Text.RegularExpressions;
using Application.Exceptions;
using Identity.Application.Contracts;
using Identity.Application.Users.Commands;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Users.Commands;

public sealed partial class UpdateUserCommandHandler(IdentityDbContext identityDbContext) : IUpdateUserCommandHandler
{
    public async Task<UserListItemDto> HandleAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        // ── Validation ──────────────────────────────────────────────
        var errors = new Dictionary<string, string[]>();

        if (userId <= 0)
            errors["userId"] = ["INVALID_USER_ID"];

        if (string.IsNullOrWhiteSpace(request.Email))
            errors["email"] = ["EMAIL_REQUIRED"];
        else if (!EmailRegex().IsMatch(request.Email.Trim()))
            errors["email"] = ["INVALID_EMAIL"];

        if (!string.IsNullOrWhiteSpace(request.ProfileImageUrl) && !Uri.TryCreate(request.ProfileImageUrl, UriKind.Absolute, out _))
            errors["profileImageUrl"] = ["INVALID_URL"];

        if (errors.Count > 0)
            throw new ValidationAppException("USER_UPDATE_VALIDATION_FAILED", errors);

        // ── Fetch ───────────────────────────────────────────────────
        var user = await identityDbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);
        if (user is null)
            throw new NotFoundAppException("USER_NOT_FOUND");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // ── Username (optional update) ──────────────────────────────
        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            var normalizedUsername = request.Username.Trim();
            var duplicateUsername = await identityDbContext.Users
                .AsNoTracking()
                .AnyAsync(x => !x.IsDeleted && x.Id != userId && x.Username == normalizedUsername, cancellationToken);
            if (!duplicateUsername)
                user.Username = normalizedUsername;
        }

        // ── Email uniqueness ────────────────────────────────────────
        var duplicateEmail = await identityDbContext.Users
            .AsNoTracking()
            .AnyAsync(x => !x.IsDeleted && x.Id != userId && x.Email == normalizedEmail, cancellationToken);

        if (duplicateEmail)
            throw new ValidationAppException("EMAIL_TAKEN", new Dictionary<string, string[]>
            {
                ["email"] = ["EMAIL_TAKEN"]
            });

        // ── Apply ───────────────────────────────────────────────────
        user.FirstName = request.FirstName?.Trim();
        user.LastName = request.LastName?.Trim();
        user.Email = normalizedEmail;
        user.IsActive = request.IsActive;
        user.ProfileImageUrl = request.ProfileImageUrl?.Trim();
        user.MustChangePassword = request.MustChangePassword;
        // PasswordExpiresAt is system-managed only (set on password change)

        await identityDbContext.SaveChangesAsync(cancellationToken);

        // ── Return ──────────────────────────────────────────────────
        return await identityDbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new UserListItemDto(
                x.Id,
                x.UserCode,
                x.Username,
                x.FirstName,
                x.LastName,
                string.IsNullOrWhiteSpace(x.FirstName) && string.IsNullOrWhiteSpace(x.LastName)
                    ? x.Username
                    : ((x.FirstName ?? "") + " " + (x.LastName ?? "")).Trim(),
                x.Email,
                x.IsActive,
                x.MustChangePassword,
                x.PasswordExpiresAt,
                x.CreatedAt,
                x.CreatedBy,
                x.ModifiedBy,
                x.ModifiedAt,
                x.IsDeleted,
                x.DeletedAt,
                x.DeletedBy,
                x.ProfileImageUrl,
                0,
                null))
            .FirstAsync(cancellationToken);
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();
}
