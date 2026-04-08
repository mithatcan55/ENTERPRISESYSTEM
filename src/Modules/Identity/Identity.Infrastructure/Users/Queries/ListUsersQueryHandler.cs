using Identity.Application.Contracts;
using Identity.Application.Users.Queries;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Users.Queries;

public sealed class ListUsersQueryHandler(IdentityDbContext identityDbContext) : IListUsersQueryHandler
{
    public async Task<PagedResult<UserListItemDto>> HandleAsync(ListUsersQuery query, CancellationToken cancellationToken)
    {
        var q = identityDbContext.Users.AsNoTracking().AsQueryable();

        if (!query.IncludeDeleted)
            q = q.Where(x => !x.IsDeleted);

        if (query.IsActive.HasValue)
            q = q.Where(x => x.IsActive == query.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(x =>
                x.UserCode.ToLower().Contains(term) ||
                x.Email.ToLower().Contains(term));
        }

        q = (query.SortBy?.ToLower(), query.SortDirection?.ToLower()) switch
        {
            ("usercode", "asc") => q.OrderBy(x => x.UserCode),
            ("usercode", _) => q.OrderByDescending(x => x.UserCode),
            ("email", "asc") => q.OrderBy(x => x.Email),
            ("email", _) => q.OrderByDescending(x => x.Email),
            ("createdat", "asc") => q.OrderBy(x => x.CreatedAt),
            _ => q.OrderByDescending(x => x.CreatedAt)
        };

        var totalCount = await q.CountAsync(cancellationToken);

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var userIds = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        // Rol bilgisi icin ayri join; LINQ GroupJoin null-safe sekilde yazilir.
        var rolesByUser = await (
            from ur in identityDbContext.UserRoles.AsNoTracking()
            join r in identityDbContext.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where userIds.Contains(ur.UserId) && !ur.IsDeleted
            select new { ur.UserId, r.Name }
        ).ToListAsync(cancellationToken);

        var roleLookup = rolesByUser
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var users = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = users.Select(x =>
        {
            var roles = roleLookup.TryGetValue(x.Id, out var r) ? r : [];
            var roleCount = roles.Count;
            var primaryRole = roles.FirstOrDefault()?.Name;

            var displayName = string.IsNullOrWhiteSpace(x.FirstName) && string.IsNullOrWhiteSpace(x.LastName)
                ? x.UserCode
                : $"{x.FirstName} {x.LastName}".Trim();

            return new UserListItemDto(
                x.Id,
                x.UserCode,
                x.FirstName,
                x.LastName,
                displayName,
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
                roleCount,
                primaryRole);
        }).ToList();

        return new PagedResult<UserListItemDto>(items, totalCount, page, pageSize);
    }
}

