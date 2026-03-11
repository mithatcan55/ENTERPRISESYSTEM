namespace Application.Pipeline;

public interface IPermissionProtectedRequest
{
    string PermissionCode { get; }
}
