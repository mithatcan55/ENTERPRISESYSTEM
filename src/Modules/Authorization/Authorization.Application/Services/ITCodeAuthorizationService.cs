using Authorization.Application.Contracts;

namespace Authorization.Application.Services;

public interface ITCodeAuthorizationService
{
    Task<TCodeAccessResult> AuthorizeAsync(
        string transactionCode,
        int userId,
        int companyId,
        IReadOnlyDictionary<string, string?> contextValues,
        string? requiredActionCode,
        bool denyOnUnsatisfiedConditions,
        CancellationToken cancellationToken);
}
