using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.CodeAnalysis;

namespace Authorization.Application.Security;

public sealed class TCodeRequirement(string transactionCode) : IAuthorizationRequirement
{
    public string TransactionCode { get; } = transactionCode;
    public string? RequiredActionCode { get; init; }
    public bool DenyOnUnsatisfiedConditions { get; init; } = true;
}

public sealed class TCodeAuthorizeAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "TCode:";
    public string? ActionCode { get; }
    public bool DenyOnUnsatisfiedConditions { get; init; } = true;

    public TCodeAuthorizeAttribute([DisallowNull] string transactionCode, string? actionCode = null)
    {
        if (string.IsNullOrWhiteSpace(transactionCode))
        {
            throw new ArgumentException("Transaction code bos olamaz.", nameof(transactionCode));
        }

        ActionCode = string.IsNullOrWhiteSpace(actionCode)
            ? null
            : actionCode.Trim().ToUpperInvariant();

        Policy = $"{PolicyPrefix}{transactionCode.Trim().ToUpperInvariant()}";
    }
}
