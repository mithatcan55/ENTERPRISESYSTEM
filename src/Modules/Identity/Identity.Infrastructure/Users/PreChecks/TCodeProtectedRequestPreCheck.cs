using Application.Exceptions;
using Application.Pipeline;
using Application.Security;
using Authorization.Application.Services;

namespace Identity.Infrastructure.Users.PreChecks;

/// <summary>
/// Users akisinda controller attribute'ine ek olarak command/query seviyesinde
/// ikinci savunma hattini calistirir. Boylece endpoint disinda baska bir giris
/// noktasi olussa bile T-Code kontrolu korunur.
/// </summary>
public sealed class TCodeProtectedRequestPreCheck<TRequest>(
    ITCodeAuthorizationService tCodeAuthorizationService,
    ICurrentUserContext currentUserContext) : IRequestPreCheck<TRequest>
    where TRequest : ITCodeProtectedRequest
{
    public async Task CheckAsync(TRequest request, CancellationToken cancellationToken)
    {
        if (!currentUserContext.TryGetUserId(out var userId) || !currentUserContext.TryGetCompanyId(out var companyId))
        {
            throw new ForbiddenAppException("Kimligi dogrulanmis kullanici veya sirket baglami cozumlenemedi.");
        }

        var authorizationResult = await tCodeAuthorizationService.AuthorizeAsync(
            request.TransactionCode,
            userId,
            companyId,
            request.ContextValues,
            request.ActionCode,
            request.DenyOnUnsatisfiedConditions,
            cancellationToken);

        if (!authorizationResult.IsAllowed)
        {
            throw new ForbiddenAppException(
                authorizationResult.DeniedReason ?? "T-Code yetki kontrolu basarisiz.",
                errorCode: "tcode_request_precheck_failed");
        }
    }
}
