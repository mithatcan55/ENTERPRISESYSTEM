using Application.Pipeline;
using Identity.Application.Contracts;

namespace Identity.Application.Permissions.Queries;

public sealed record ListUserActionPermissionsQuery(UserActionPermissionQueryRequest Request) : IAdminOnlyRequest;
