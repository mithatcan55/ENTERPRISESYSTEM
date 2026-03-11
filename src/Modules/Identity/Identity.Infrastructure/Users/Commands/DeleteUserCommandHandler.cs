using Application.Exceptions;
using Identity.Application.Users.Commands;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Users.Commands;

public sealed class DeleteUserCommandHandler(BusinessDbContext businessDbContext) : IDeleteUserCommandHandler
{
    public async Task HandleAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await businessDbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);
        if (user is null)
            throw new NotFoundAppException($"Kullanici bulunamadi. userId={userId}");

        user.IsActive = false;
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;

        await businessDbContext.SaveChangesAsync(cancellationToken);
    }
}
