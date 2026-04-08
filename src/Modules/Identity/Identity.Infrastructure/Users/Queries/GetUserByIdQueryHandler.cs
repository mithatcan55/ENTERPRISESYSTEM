using Application.Exceptions;
using Identity.Application.Contracts;
using Identity.Application.Users.Queries;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Users.Queries;

public sealed class GetUserByIdQueryHandler(
    IdentityDbContext identityDbContext,
    AuthorizationDbContext authorizationDbContext) : IGetUserByIdQueryHandler
{
    public async Task<UserDetailDto> HandleAsync(int userId, CancellationToken cancellationToken)
    {
        var userEntity = await identityDbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (userEntity is null)
            throw new NotFoundAppException($"Kullanici bulunamadi. userId={userId}");

        // Roles
        var roles = await (
            from ur in identityDbContext.UserRoles.AsNoTracking()
            join r in identityDbContext.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where ur.UserId == userId && !ur.IsDeleted && !r.IsDeleted
            select new UserRoleDto(r.Id, r.Code, r.Name)
        ).ToListAsync(cancellationToken);

        // Direct permissions (UserPageActionPermission)
        var directPerms = await (
            from ap in authorizationDbContext.UserPageActionPermissions.AsNoTracking()
            join p in authorizationDbContext.SubModulePages.AsNoTracking() on ap.SubModulePageId equals p.Id
            where ap.UserId == userId && !ap.IsDeleted
            select new UserDirectPermissionDto(ap.Id, ap.SubModulePageId, p.TransactionCode, ap.ActionCode, ap.IsAllowed)
        ).ToListAsync(cancellationToken);

        var displayName = string.IsNullOrWhiteSpace(userEntity.FirstName) && string.IsNullOrWhiteSpace(userEntity.LastName)
            ? userEntity.Username
            : $"{userEntity.FirstName} {userEntity.LastName}".Trim();

        return new UserDetailDto(
            userEntity.Id,
            userEntity.UserCode,
            userEntity.Username,
            userEntity.FirstName,
            userEntity.LastName,
            displayName,
            userEntity.Email,
            userEntity.IsActive,
            userEntity.MustChangePassword,
            userEntity.PasswordExpiresAt,
            userEntity.CreatedAt,
            userEntity.CreatedBy,
            userEntity.ModifiedBy,
            userEntity.ModifiedAt,
            userEntity.IsDeleted,
            userEntity.DeletedAt,
            userEntity.DeletedBy,
            userEntity.ProfileImageUrl,
            roles,
            directPerms);
    }
}
