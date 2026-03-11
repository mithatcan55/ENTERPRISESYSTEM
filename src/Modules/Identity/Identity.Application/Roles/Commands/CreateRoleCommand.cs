using Application.Pipeline;
using Identity.Application.Contracts;

namespace Identity.Application.Roles.Commands;

public sealed record CreateRoleCommand(CreateRoleRequest Request) : IAdminOnlyRequest;
