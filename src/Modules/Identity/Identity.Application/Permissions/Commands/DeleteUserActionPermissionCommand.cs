using Application.Pipeline;

namespace Identity.Application.Permissions.Commands;

public sealed record DeleteUserActionPermissionCommand(int PermissionId) : IAdminOnlyRequest, IPermissionProtectedRequest
{
    public string PermissionCode => "PERMISSIONS_DELETE";
}
