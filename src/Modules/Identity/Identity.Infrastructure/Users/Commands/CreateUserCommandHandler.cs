using Application.Exceptions;
using Identity.Application.Configuration;
using Identity.Application.Contracts;
using Identity.Application.Services;
using Identity.Application.Users.Commands;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Authorization;
using Infrastructure.Persistence.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Users.Commands;

public sealed class CreateUserCommandHandler(
    IdentityDbContext identityDbContext,
    AuthorizationDbContext authorizationDbContext,
    IPasswordPolicyService passwordPolicyService,
    IIdentityNotificationService identityNotificationService,
    IOptions<PasswordPolicyOptions> passwordPolicyOptions) : ICreateUserCommandHandler
{
    public async Task<CreatedUserDto> HandleAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        // Buradaki kontroller request formatindan cok kullanici olusturma senaryosunun is kurallarina odaklanir.
        var validationErrors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.UserCode))
            validationErrors["userCode"] = ["UserCode zorunludur."];

        if (string.IsNullOrWhiteSpace(request.Email))
            validationErrors["email"] = ["Email zorunludur."];

        if (string.IsNullOrWhiteSpace(request.Password))
            validationErrors["password"] = ["Password zorunludur."];

        if (request.CompanyId <= 0)
            validationErrors["companyId"] = ["CompanyId pozitif bir deger olmalidir."];

        if (request.NotifyAdminByMail && string.IsNullOrWhiteSpace(request.AdminEmail))
            validationErrors["adminEmail"] = ["NotifyAdminByMail=true ise adminEmail zorunludur."];

        if (validationErrors.Count > 0)
            throw new ValidationAppException("Kullanici olusturma dogrulamasi basarisiz.", validationErrors);

        var normalizedUserCode = request.UserCode.Trim().ToUpperInvariant();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // Sifre karmasikligi teknik bir input kontrolu degil, merkezi policy kuralidir.
        await passwordPolicyService.ValidateComplexityOrThrowAsync(request.Password, normalizedUserCode, normalizedEmail, cancellationToken);

        var duplicateExists = await identityDbContext.Users
            .AsNoTracking()
            .AnyAsync(x => !x.IsDeleted &&
                           (x.UserCode == normalizedUserCode
                            || x.Email == normalizedEmail),
                cancellationToken);

        if (duplicateExists)
        {
            throw new ValidationAppException(
                "Kullanici benzersizlik kontrolu basarisiz.",
                new Dictionary<string, string[]>
                {
                    ["user"] = ["Ayni userCode veya email ile kayit zaten mevcut."]
                });
        }

        var requestedRoleIds = request.RoleIds?.Distinct().ToList() ?? [];
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

        var requestedPermissionIds = request.PermissionIds?.Distinct().ToList() ?? [];
        var validPermissionIds = requestedPermissionIds.Count == 0
            ? []
            : await authorizationDbContext.SubModulePages
                .AsNoTracking()
                .Where(p => !p.IsDeleted && requestedPermissionIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

        var invalidPermissionIds = requestedPermissionIds.Except(validPermissionIds).ToList();
        if (invalidPermissionIds.Count > 0)
        {
            throw new ValidationAppException(
                "INVALID_PERMISSION_IDS",
                new Dictionary<string, string[]>
                {
                    ["permissionIds"] = [$"Gecersiz yetki ID'leri: {string.Join(", ", invalidPermissionIds)}."]
                });
        }

        var systemModule = await authorizationDbContext.Modules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == "SYS" && !x.IsDeleted, cancellationToken);

        if (systemModule is null)
            throw new NotFoundAppException("Varsayilan SYS modulu bulunamadi.");

        var user = new User
        {
            UserCode = normalizedUserCode,
            FirstName = request.FirstName?.Trim(),
            LastName = request.LastName?.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = true,
            MustChangePassword = true,
            PasswordExpiresAt = passwordPolicyOptions.Value.PasswordExpiryDays > 0
                ? DateTime.UtcNow.AddDays(passwordPolicyOptions.Value.PasswordExpiryDays)
                : null
        };

        identityDbContext.Users.Add(user);
        await identityDbContext.SaveChangesAsync(cancellationToken);

        await passwordPolicyService.RecordPasswordHistoryAsync(user.Id, user.PasswordHash, cancellationToken);

        try
        {
            authorizationDbContext.UserModulePermissions.Add(new UserModulePermission
            {
                UserId = user.Id,
                ModuleId = systemModule.Id,
                AuthorizationLevel = 1
            });

            authorizationDbContext.UserCompanyPermissions.Add(new UserCompanyPermission
            {
                UserId = user.Id,
                CompanyId = request.CompanyId,
                AuthorizationLevel = 4
            });

            await authorizationDbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await identityDbContext.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM \"{PersistenceSchemaNames.Business}\".\"UserPasswordHistories\" WHERE \"UserId\" = {user.Id}",
                cancellationToken);
            await identityDbContext.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM \"{PersistenceSchemaNames.Business}\".\"Users\" WHERE \"Id\" = {user.Id}",
                cancellationToken);
            throw;
        }

        // Assign roles if provided
        if (validRoleIds.Count > 0)
        {
            foreach (var roleId in validRoleIds)
            {
                identityDbContext.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = roleId
                });
            }

            await identityDbContext.SaveChangesAsync(cancellationToken);
        }

        // Assign direct permissions if provided (UserPageActionPermission)
        if (validPermissionIds.Count > 0)
        {
            foreach (var pageId in validPermissionIds)
            {
                authorizationDbContext.UserPageActionPermissions.Add(new UserPageActionPermission
                {
                    UserId = user.Id,
                    SubModulePageId = pageId,
                    ActionCode = "VIEW",
                    IsAllowed = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await authorizationDbContext.SaveChangesAsync(cancellationToken);
        }

        if (request.NotifyAdminByMail)
        {
            // Bildirim ana veri kaydindan sonra cagriliyor.
            // Boylece mail problemi kullanici kaydini geri sardirmaz.
            await identityNotificationService.QueueAdminMailAsync(
                request.AdminEmail!.Trim(),
                $"Yeni kullanici olusturuldu: {user.UserCode}",
                $"Kullanici olusturuldu. UserCode={user.UserCode}, Email={user.Email}",
                cancellationToken);
        }

        return new CreatedUserDto(
            user.Id,
            user.UserCode,
            user.FirstName,
            user.LastName,
            user.Email,
            user.MustChangePassword,
            user.PasswordExpiresAt);
    }
}
