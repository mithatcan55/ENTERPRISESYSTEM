using Host.Api.Exceptions;
using Host.Api.Identity.Configuration;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Host.Api.Identity.Services;

public sealed class PasswordPolicyService(
    BusinessDbContext businessDbContext,
    IOptions<PasswordPolicyOptions> passwordPolicyOptions) : IPasswordPolicyService
{
    private readonly PasswordPolicyOptions _options = passwordPolicyOptions.Value;

    public void ValidateComplexityOrThrow(string password, string? username, string? email)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password) || password.Length < _options.MinLength)
        {
            errors.Add($"Şifre en az {_options.MinLength} karakter olmalıdır.");
        }

        if (_options.RequireUppercase && !password.Any(char.IsUpper))
        {
            errors.Add("Şifre en az 1 büyük harf içermelidir.");
        }

        if (_options.RequireLowercase && !password.Any(char.IsLower))
        {
            errors.Add("Şifre en az 1 küçük harf içermelidir.");
        }

        if (_options.RequireDigit && !password.Any(char.IsDigit))
        {
            errors.Add("Şifre en az 1 rakam içermelidir.");
        }

        if (_options.RequireSpecialCharacter && !password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            errors.Add("Şifre en az 1 özel karakter içermelidir.");
        }

        if (!string.IsNullOrWhiteSpace(username) && password.Contains(username, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Şifre kullanıcı adını içeremez.");
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            var localPart = email.Split('@', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(localPart) && password.Contains(localPart, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("Şifre email bilgisini içeremez.");
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationAppException(
                "Şifre politikası doğrulaması başarısız.",
                new Dictionary<string, string[]>
                {
                    ["password"] = errors.ToArray()
                });
        }
    }

    public async Task EnsureNotRecentlyUsedOrThrowAsync(int userId, string candidatePassword, CancellationToken cancellationToken)
    {
        var recentHashes = await businessDbContext.UserPasswordHistories
            .AsNoTracking()
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .OrderByDescending(x => x.ChangedAt)
            .ThenByDescending(x => x.Id)
            .Take(_options.HistoryCount)
            .Select(x => x.PasswordHash)
            .ToListAsync(cancellationToken);

        if (recentHashes.Any(hash => BCrypt.Net.BCrypt.Verify(candidatePassword, hash)))
        {
            throw new ValidationAppException(
                "Şifre politikası doğrulaması başarısız.",
                new Dictionary<string, string[]>
                {
                    ["password"] = [$"Yeni şifre son {_options.HistoryCount} şifre ile aynı olamaz."]
                });
        }
    }

    public async Task EnsureMinimumPasswordAgeOrThrowAsync(int userId, CancellationToken cancellationToken)
    {
        if (_options.MinimumPasswordAgeMinutes <= 0)
        {
            return;
        }

        var lastChangedAt = await businessDbContext.UserPasswordHistories
            .AsNoTracking()
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .OrderByDescending(x => x.ChangedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => (DateTime?)x.ChangedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (!lastChangedAt.HasValue)
        {
            return;
        }

        var nextAllowedChangeAt = lastChangedAt.Value.AddMinutes(_options.MinimumPasswordAgeMinutes);
        if (DateTime.UtcNow < nextAllowedChangeAt)
        {
            throw new ValidationAppException(
                "Şifre politikası doğrulaması başarısız.",
                new Dictionary<string, string[]>
                {
                    ["password"] = [$"Şifre çok sık değiştirilemez. En erken {nextAllowedChangeAt:O} tarihinde tekrar deneyin."]
                });
        }
    }

    public async Task RecordPasswordHistoryAsync(int userId, string passwordHash, CancellationToken cancellationToken)
    {
        businessDbContext.UserPasswordHistories.Add(new UserPasswordHistory
        {
            UserId = userId,
            PasswordHash = passwordHash,
            ChangedAt = DateTime.UtcNow
        });

        await businessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task EnforcePasswordChangePolicyOrThrowAsync(User user, string candidatePassword, CancellationToken cancellationToken)
    {
        ValidateComplexityOrThrow(candidatePassword, user.Username, user.Email);

        if (BCrypt.Net.BCrypt.Verify(candidatePassword, user.PasswordHash))
        {
            throw new ValidationAppException(
                "Şifre politikası doğrulaması başarısız.",
                new Dictionary<string, string[]>
                {
                    ["password"] = ["Yeni şifre mevcut şifre ile aynı olamaz."]
                });
        }

        await EnsureMinimumPasswordAgeOrThrowAsync(user.Id, cancellationToken);
        await EnsureNotRecentlyUsedOrThrowAsync(user.Id, candidatePassword, cancellationToken);
    }
}
