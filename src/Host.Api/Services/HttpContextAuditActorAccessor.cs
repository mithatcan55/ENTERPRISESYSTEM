using Infrastructure.Persistence.Auditing;

namespace Host.Api.Services;

/// <summary>
/// İstek bağlamındaki kullanıcı bilgisini alır ve audit alanlarına taşır.
/// Kullanıcı yoksa sistem işlemlerinin izlenebilmesi için sabit bir değer döner.
/// </summary>
public sealed class HttpContextAuditActorAccessor(ICurrentUserContext currentUserContext) : IAuditActorAccessor
{
    public string GetActorId()
    {
        return currentUserContext.TryGetActorIdentity(out var actorIdentity)
            ? actorIdentity
            : "system";
    }
}
