using Application.Pipeline;

namespace Identity.Application.Roles.Queries;

public sealed record ListUserRolesQuery(int UserId) : IAdminOnlyRequest;
