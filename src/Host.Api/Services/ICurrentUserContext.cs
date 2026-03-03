namespace Host.Api.Services;

/// <summary>
/// İstek bağlamındaki kullanıcı ve şirket bilgisini güvenli şekilde çözmek için kullanılır.
/// </summary>
public interface ICurrentUserContext
{
    bool TryGetUserId(out int userId);
    bool TryGetCompanyId(out int companyId);
    bool TryGetUserCode(out string userCode);
    bool TryGetUsername(out string username);
    bool TryGetActorIdentity(out string actorIdentity);
}
