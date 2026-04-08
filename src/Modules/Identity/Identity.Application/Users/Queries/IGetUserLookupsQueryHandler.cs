using Identity.Application.Contracts;

namespace Identity.Application.Users.Queries;

public interface IGetUserLookupsQueryHandler
{
    Task<UserLookupsDto> HandleAsync(CancellationToken cancellationToken);
}
