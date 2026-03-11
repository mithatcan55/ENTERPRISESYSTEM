using Application.Exceptions;
using Identity.Application.Contracts;
using Identity.Application.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Authorization;
using Infrastructure.Persistence.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Services;

public sealed class UserManagementService(
    BusinessDbContext businessDbContext,
    IPasswordPolicyService passwordPolicyService,
    IIdentityNotificationService identityNotificationService) : IUserManagementService
{
    public async Task<IReadOnlyList<UserListItemDto>> ListAsync(CancellationToken cancellationToken)
    {
        return await businessDbContext.Users
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new UserListItemDto(
                x.Id,
                x.UserCode,
                x.Username,
                x.Email,
                x.IsActive,
                x.MustChangePassword,
                x.PasswordExpiresAt,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<CreatedUserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
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
            validationErrors["companyId"] = ["CompanyId pozitif bir değer olmalıdır."];

        if (request.NotifyAdminByMail && string.IsNullOrWhiteSpace(request.AdminEmail))
            validationErrors["adminEmail"] = ["NotifyAdminByMail=true ise adminEmail zorunludur."];

        if (validationErrors.Count > 0)
            throw new ValidationAppException("Kullanıcı oluşturma doğrulaması başarısız.", validationErrors);

        var normalizedUserCode = request.UserCode.Trim().ToUpperInvariant();
        var normalizedUsername = request.Username.Trim();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        passwordPolicyService.ValidateComplexityOrThrow(request.Password, normalizedUsername, normalizedEmail);

        var duplicateExists = await businessDbContext.Users
            .AsNoTracking()
            .AnyAsync(x => !x.IsDeleted &&
                           (x.UserCode == normalizedUserCode
                            || x.Username == normalizedUsername
                            || x.Email == normalizedEmail),
                cancellationToken);

        if (duplicateExists)
        {
            throw new ValidationAppException(
                "Kullanıcı benzersizlik kontrolü başarısız.",
                new Dictionary<string, string[]>
                {
                    ["user"] = ["Aynı userCode, username veya email ile kayıt zaten mevcut."]
                });
        }

        var systemModule = await businessDbContext.Modules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == "SYS" && !x.IsDeleted, cancellationToken);

        if (systemModule is null)
            throw new NotFoundAppException("Varsayılan SYS modülü bulunamadı.");

        await using var transaction = await businessDbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
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

            businessDbContext.Users.Add(user);
            await businessDbContext.SaveChangesAsync(cancellationToken);

            await passwordPolicyService.RecordPasswordHistoryAsync(user.Id, user.PasswordHash, cancellationToken);

            businessDbContext.UserModulePermissions.Add(new UserModulePermission
            {
                UserId = user.Id,
                ModuleId = systemModule.Id,
                AuthorizationLevel = 1
            });
            await businessDbContext.SaveChangesAsync(cancellationToken);

            businessDbContext.UserCompanyPermissions.Add(new UserCompanyPermission
            {
                UserId = user.Id,
                CompanyId = request.CompanyId,
                AuthorizationLevel = 4
            });
            await businessDbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            if (request.NotifyAdminByMail)
            {
                await identityNotificationService.QueueAdminMailAsync(
                    request.AdminEmail!.Trim(),
                    $"Yeni kullanıcı oluşturuldu: {user.UserCode}",
                    $"Kullanıcı oluşturuldu. UserCode={user.UserCode}, Username={user.Username}, Email={user.Email}",
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
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<UserListItemDto> UpdateAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (userId <= 0)
        {
            throw new ValidationAppException(
                "Kullanıcı güncelleme doğrulaması başarısız.",
                new Dictionary<string, string[]>
                {
                    ["userId"] = ["Geçerli bir userId zorunludur."]
                });
        }

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ValidationAppException(
                "Kullanıcı güncelleme doğrulaması başarısız.",
                new Dictionary<string, string[]>
                {
                    ["username"] = ["Username zorunludur."],
                    ["email"] = ["Email zorunludur."]
                });
        }

        var user = await businessDbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);
        if (user is null)
            throw new NotFoundAppException($"Kullanıcı bulunamadı. userId={userId}");

        var normalizedUsername = request.Username.Trim();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var duplicateExists = await businessDbContext.Users
            .AsNoTracking()
            .AnyAsync(x => !x.IsDeleted && x.Id != userId && (x.Username == normalizedUsername || x.Email == normalizedEmail), cancellationToken);

        if (duplicateExists)
        {
            throw new ValidationAppException(
                "Kullanıcı benzersizlik kontrolü başarısız.",
                new Dictionary<string, string[]>
                {
                    ["user"] = ["Aynı username veya email ile kayıt zaten mevcut."]
                });
        }

        user.Username = normalizedUsername;
        user.Email = normalizedEmail;
        user.IsActive = request.IsActive;

        await businessDbContext.SaveChangesAsync(cancellationToken);

        return await businessDbContext.Users
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

    public async Task DeactivateAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await businessDbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);
        if (user is null)
            throw new NotFoundAppException($"Kullanıcı bulunamadı. userId={userId}");

        user.IsActive = false;
        await businessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ReactivateAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await businessDbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);
        if (user is null)
            throw new NotFoundAppException($"Kullanıcı bulunamadı. userId={userId}");

        user.IsActive = true;
        await businessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await businessDbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);
        if (user is null)
            throw new NotFoundAppException($"Kullanıcı bulunamadı. userId={userId}");

        user.IsActive = false;
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;

        await businessDbContext.SaveChangesAsync(cancellationToken);
    }
}
