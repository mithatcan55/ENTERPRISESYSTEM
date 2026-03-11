using Application.Pipeline;

namespace Identity.Application.Roles.Commands;

public sealed record UnassignRoleCommand(int UserId, int RoleId) : IAdminOnlyRequest;
