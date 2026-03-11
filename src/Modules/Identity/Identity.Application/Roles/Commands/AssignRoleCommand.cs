using Application.Pipeline;

namespace Identity.Application.Roles.Commands;

public sealed record AssignRoleCommand(int UserId, int RoleId) : IAdminOnlyRequest;
