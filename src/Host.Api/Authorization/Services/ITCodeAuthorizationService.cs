using Host.Api.Authorization.Models;

namespace Host.Api.Authorization.Services;

/// <summary>
/// T-Code tabanlı erişim kararını üretir.
/// </summary>
public interface ITCodeAuthorizationService
{
    Task<TCodeAccessResult> AuthorizeAsync(
        string transactionCode,
        int userId,
        int companyId,
        IReadOnlyDictionary<string, string?> contextValues,
        CancellationToken cancellationToken);
}
