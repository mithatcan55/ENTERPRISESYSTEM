using Application.Exceptions;
using Identity.Application.Contracts;
using Identity.Application.Users.Queries;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Users.Queries;

public sealed class GetUserByIdQueryHandler(IdentityDbContext identityDbContext) : IGetUserByIdQueryHandler
{
    public async Task<UserDetailDto> HandleAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await identityDbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new UserDetailDto(
                x.Id,
                x.UserCode,
                x.Username,
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
                x.ProfileImageUrl))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            throw new NotFoundAppException($"Kullanici bulunamadi. userId={userId}");

        return user;
    }
}
