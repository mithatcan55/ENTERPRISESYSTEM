using System.Text.RegularExpressions;
using Application.Exceptions;
using Identity.Application.Contracts;
using Identity.Application.Users.Commands;
using Identity.Infrastructure.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Authorization;
using Infrastructure.Persistence.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Users.Commands;

public sealed partial class UpdateUserCommandHandler(
    IdentityDbContext identityDbContext,
    AuthorizationDbContext authorizationDbContext) : IUpdateUserCommandHandler
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
        var wasActive = user.IsActive;

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // ── Email uniqueness ────────────────────────────────────────
        var duplicateEmail = await identityDbContext.Users
            .AsNoTracking()
            .AnyAsync(x => !x.IsDeleted && x.Id != userId && x.Email == normalizedEmail, cancellationToken);

        if (duplicateEmail)
            throw new ValidationAppException("EMAIL_TAKEN", new Dictionary<string, string[]>
            {
                ["email"] = ["EMAIL_TAKEN"]
            });

        // ── Apply basic fields ──────────────────────────────────────
        user.FirstName = request.FirstName?.Trim();
        user.LastName = request.LastName?.Trim();
        user.Email = normalizedEmail;
        user.IsActive = request.IsActive;
        user.ProfileImageUrl = request.ProfileImageUrl?.Trim();
        user.MustChangePassword = request.MustChangePassword;

        await identityDbContext.SaveChangesAsync(cancellationToken);

        if (wasActive && !user.IsActive)
        {
            await identityDbContext.RevokeAllSessionsAndRefreshTokensAsync(userId, "critical_change:user_deactivate", cancellationToken);
        }

        // ── Replace roles (if provided) ─────────────────────────────
        if (request.RoleIds is not null)
        {
            var requestedRoleIds = request.RoleIds.Distinct().ToList();
            var validRoleIds = requestedRoleIds.Count == 0
                ? []
                : await identityDbContext.Roles
                    .AsNoTracking()
                    .Where(r => !r.IsDeleted && requestedRoleIds.Contains(r.Id))
                    .Select(r => r.Id)
                    .ToListAsync(cancellationToken);

            var invalidRoleIds = requestedRoleIds.Except(validRoleIds).ToList();
            if (invalidRoleIds.Count > 0)
            {
                throw new ValidationAppException(
                    "INVALID_ROLE_IDS",
                    new Dictionary<string, string[]>
                    {
                        ["roleIds"] = [$"Gecersiz rol ID'leri: {string.Join(", ", invalidRoleIds)}."]
                    });
            }

            var existingRoles = await identityDbContext.UserRoles
                .Where(ur => ur.UserId == userId)
                .ToListAsync(cancellationToken);

            foreach (var existing in existingRoles)
            {
                if (requestedRoleIds.Contains(existing.RoleId))
                {
                    if (existing.IsDeleted)
                    {
                        existing.IsDeleted = false;
                        existing.DeletedAt = null;
                    }
                }
                else if (!existing.IsDeleted)
                {
                    existing.IsDeleted = true;
                    existing.DeletedAt = DateTime.UtcNow;
                }
            }

            var existingRoleIds = existingRoles.Where(r => !r.IsDeleted).Select(r => r.RoleId).ToHashSet();
            var validNewRoleIds = validRoleIds.Where(id => !existingRoleIds.Contains(id)).ToList();

            foreach (var roleId in validNewRoleIds)
                identityDbContext.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });

            await identityDbContext.SaveChangesAsync(cancellationToken);
            await identityDbContext.RevokeAllSessionsAndRefreshTokensAsync(userId, "critical_change:role_change", cancellationToken);
        }

        // ── Replace direct permissions (if provided) ────────────────
        if (request.PermissionIds is not null)
        {
            var requestedPermissionIds = request.PermissionIds.Distinct().ToList();
            if (requestedPermissionIds.Count > 0)
            {
                var validPermissionIds = await authorizationDbContext.SubModulePages
                    .AsNoTracking()
                    .Where(p => p.Id > 0 && !p.IsDeleted && requestedPermissionIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync(cancellationToken);

                var invalidPageIds = requestedPermissionIds.Except(validPermissionIds).ToList();

                if (invalidPageIds.Count > 0)
                {
                    throw new ValidationAppException(
                        "INVALID_PERMISSION_IDS",
                        new Dictionary<string, string[]>
                        {
                            ["permissionIds"] = [$"Gecersiz yetki ID'leri: {string.Join(", ", invalidPageIds)}."]
                        });
                }
            }

            var existingPerms = await authorizationDbContext.UserPageActionPermissions
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .ToListAsync(cancellationToken);

            // Soft-delete permissions not in the new list
            foreach (var existing in existingPerms)
            {
                if (!requestedPermissionIds.Contains(existing.SubModulePageId))
                {
                    existing.IsDeleted = true;
                    existing.DeletedAt = DateTime.UtcNow;
                }
            }

            await authorizationDbContext.SaveChangesAsync(cancellationToken);

            // Add new permissions
            var existingPageIds = existingPerms.Where(p => !p.IsDeleted).Select(p => p.SubModulePageId).ToHashSet();

            foreach (var pageId in requestedPermissionIds)
            {
                if (existingPageIds.Contains(pageId))
                    continue; // Already assigned

                authorizationDbContext.UserPageActionPermissions.Add(new UserPageActionPermission
                {
                    UserId = userId,
                    SubModulePageId = pageId,
                    ActionCode = "ALL",
                    IsAllowed = true,
                    CreatedAt = DateTime.UtcNow
                });

                existingPageIds.Add(pageId);
            }

            await authorizationDbContext.SaveChangesAsync(cancellationToken);
            await identityDbContext.RevokeAllSessionsAndRefreshTokensAsync(userId, "critical_change:permission_change", cancellationToken);
        }

        // ── Return ──────────────────────────────────────────────────
        return await identityDbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new UserListItemDto(
                x.Id,
                x.UserCode,
                x.FirstName,
                x.LastName,
                string.IsNullOrWhiteSpace(x.FirstName) && string.IsNullOrWhiteSpace(x.LastName)
                    ? x.UserCode
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
