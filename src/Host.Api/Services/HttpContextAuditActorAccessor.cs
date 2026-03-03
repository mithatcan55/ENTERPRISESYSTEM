using Infrastructure.Persistence.Auditing;

namespace Host.Api.Services;

/// <summary>
/// İstek bağlamındaki kullanıcı bilgisini alır ve audit alanlarına taşır.
/// Kullanıcı yoksa sistem işlemlerinin izlenebilmesi için sabit bir değer döner.
/// </summary>
public sealed class HttpContextAuditActorAccessor(IHttpContextAccessor httpContextAccessor) : IAuditActorAccessor
{
    public string GetActorId()
    {
        var context = httpContextAccessor.HttpContext;
        if (context?.User?.Identity?.IsAuthenticated == true)
        {
            return context.User.FindFirst("sub")?.Value
                   ?? context.User.FindFirst("user_id")?.Value
                   ?? context.User.Identity?.Name
                   ?? "authenticated-user";
        }

        return "system";
    }
}
