using Application.Exceptions;
using Identity.Application.Contracts;
using Identity.Application.Services;
using Identity.Application.Users.Commands;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Authorization;
using Infrastructure.Persistence.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Users.Commands;

public sealed class CreateUserCommandHandler(
    IdentityDbContext identityDbContext,
    AuthorizationDbContext authorizationDbContext,
    IPasswordPolicyService passwordPolicyService,
    IIdentityNotificationService identityNotificationService) : ICreateUserCommandHandler
{
    public async Task<CreatedUserDto> HandleAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        // Buradaki kontroller request formatindan cok kullanici olusturma senaryosunun is kurallarina odaklanir.
        var validationErrors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.UserCode))
            validationErrors["userCode"] = ["UserCode zorunludur."];

        if (string.IsNullOrWhiteSpace(request.Username))
            validationErrors["username"] = ["Username zorunludur."];

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
        var normalizedUsername = request.Username.Trim();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // Sifre karmasikligi teknik bir input kontrolu degil, merkezi policy kuralidir.
        passwordPolicyService.ValidateComplexityOrThrow(request.Password, normalizedUsername, normalizedEmail);

        var duplicateExists = await identityDbContext.Users
            .AsNoTracking()
            .AnyAsync(x => !x.IsDeleted &&
                           (x.UserCode == normalizedUserCode
                            || x.Username == normalizedUsername
                            || x.Email == normalizedEmail),
                cancellationToken);

        if (duplicateExists)
        {
            throw new ValidationAppException(
                "Kullanici benzersizlik kontrolu basarisiz.",
                new Dictionary<string, string[]>
                {
                    ["user"] = ["Ayni userCode, username veya email ile kayit zaten mevcut."]
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
            Username = normalizedUsername,
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = true,
            MustChangePassword = true,
            PasswordExpiresAt = DateTime.UtcNow.AddDays(90)
        };

        identityDbContext.Users.Add(user);
        await identityDbContext.SaveChangesAsync(cancellationToken);

        await passwordPolicyService.RecordPasswordHistoryAsync(user.Id, user.PasswordHash, cancellationToken);

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

        if (request.NotifyAdminByMail)
        {
            // Bildirim ana veri kaydindan sonra cagriliyor.
            // Boylece mail problemi kullanici kaydini geri sardirmaz.
            await identityNotificationService.QueueAdminMailAsync(
                request.AdminEmail!.Trim(),
                $"Yeni kullanici olusturuldu: {user.UserCode}",
                $"Kullanici olusturuldu. UserCode={user.UserCode}, Username={user.Username}, Email={user.Email}",
                cancellationToken);
        }

        return new CreatedUserDto(
            user.Id,
            user.UserCode,
            user.Username,
            user.Email,
            user.MustChangePassword,
            user.PasswordExpiresAt);
    }
}
