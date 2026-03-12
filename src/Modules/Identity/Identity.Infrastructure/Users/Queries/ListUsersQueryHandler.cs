using Identity.Application.Contracts;
using Identity.Application.Users.Queries;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Users.Queries;

public sealed class ListUsersQueryHandler(BusinessDbContext businessDbContext) : IListUsersQueryHandler
{
    public async Task<IReadOnlyList<UserListItemDto>> HandleAsync(CancellationToken cancellationToken)
    {
        // Query handler'larda AsNoTracking varsayilan refleks olmalidir.
        // Cunku burada entity degistirmiyoruz, sadece listeleme yapiyoruz.
        return await businessDbContext.Users
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            // Yeni olusan kayitlari ustte gostermek admin ekranlari icin daha kullanislidir.
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
}
