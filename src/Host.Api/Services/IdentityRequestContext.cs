using Application.Security;
using Identity.Application.Services;

namespace Host.Api.Services;

public sealed class IdentityRequestContext(IHttpContextAccessor httpContextAccessor, ICurrentUserContext currentUserContext)
    : IIdentityRequestContext
{
    public bool TryGetUserId(out int userId) => currentUserContext.TryGetUserId(out userId);
    public bool TryGetSessionId(out int sessionId) => currentUserContext.TryGetSessionId(out sessionId);

    public bool TryGetUsername(out string username) => currentUserContext.TryGetUsername(out username);

    public bool TryGetActorIdentity(out string actorIdentity) => currentUserContext.TryGetActorIdentity(out actorIdentity);

    public bool IsInRole(string roleCode) => httpContextAccessor.HttpContext?.User.IsInRole(roleCode) == true;

    public string? RemoteIpAddress => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent => httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
}
