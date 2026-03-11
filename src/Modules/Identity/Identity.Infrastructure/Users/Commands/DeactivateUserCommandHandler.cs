using Application.Exceptions;
using Identity.Application.Users.Commands;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Users.Commands;

public sealed class DeactivateUserCommandHandler(BusinessDbContext businessDbContext) : IDeactivateUserCommandHandler
{
    public async Task HandleAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await businessDbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted, cancellationToken);
        if (user is null)
            throw new NotFoundAppException($"Kullanici bulunamadi. userId={userId}");

        user.IsActive = false;
        await businessDbContext.SaveChangesAsync(cancellationToken);
    }
}
