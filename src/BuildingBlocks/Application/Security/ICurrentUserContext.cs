namespace Application.Security;

public interface ICurrentUserContext
{
    bool TryGetUserId(out int userId);
    bool TryGetSessionId(out int sessionId);
    bool TryGetCompanyId(out int companyId);
    bool TryGetUserCode(out string userCode);
    bool TryGetUsername(out string username);
    bool TryGetActorIdentity(out string actorIdentity);
    bool IsInRole(string roleCode);
}
