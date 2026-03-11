using Application.Pipeline;

namespace Identity.Application.Roles.Queries;

public sealed record ListRolesQuery : IAdminOnlyRequest;
