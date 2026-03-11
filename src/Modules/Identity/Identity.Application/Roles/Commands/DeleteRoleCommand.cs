using Application.Pipeline;

namespace Identity.Application.Roles.Commands;

public sealed record DeleteRoleCommand(int RoleId) : IAdminOnlyRequest;
