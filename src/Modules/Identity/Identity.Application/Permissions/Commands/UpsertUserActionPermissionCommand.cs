using Application.Pipeline;
using Identity.Application.Contracts;

namespace Identity.Application.Permissions.Commands;

public sealed record UpsertUserActionPermissionCommand(UpsertUserActionPermissionRequest Request) : IAdminOnlyRequest, IPermissionProtectedRequest
{
    public string PermissionCode => "Permissions.Update";
}
