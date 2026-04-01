using Application.Exceptions;
using Identity.Application.Users.Commands;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Users.Commands;

public sealed class DeleteUserCommandHandler(IdentityDbContext identityDbContext) : IDeleteUserCommandHandler
{
    public async Task HandleAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await identityDbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);
        if (user is null)
            throw new NotFoundAppException($"Kullanici bulunamadi. userId={userId}");

        // EF Remove() kullanilmasi zorunludur: manuel IsDeleted=true atamasi EF durumunu
        // Modified'e cekmekte, bu durumda ApplyAuditRules icindeki Deleted dali calismaz
        // ve DeletedBy asla doldurulmaz. Remove() ile Deleted durumu tetiklenir.
        user.IsActive = false;
        identityDbContext.Users.Remove(user);

        await identityDbContext.SaveChangesAsync(cancellationToken);
    }
}
