namespace Identity.Application.Services;

public interface IIdentityRequestContext
{
    bool TryGetUserId(out int userId);
    bool TryGetSessionId(out int sessionId);
    bool TryGetUsername(out string username);
    bool TryGetActorIdentity(out string actorIdentity);
    bool IsInRole(string roleCode);
    string? RemoteIpAddress { get; }
    string? UserAgent { get; }
}
