using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.CodeAnalysis;

namespace Authorization.Application.Security;

public sealed class TCodeRequirement(string transactionCode) : IAuthorizationRequirement
{
    public string TransactionCode { get; } = transactionCode;
}

public sealed class TCodeAuthorizeAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "TCode:";

    public TCodeAuthorizeAttribute([DisallowNull] string transactionCode)
    {
        if (string.IsNullOrWhiteSpace(transactionCode))
        {
            throw new ArgumentException("Transaction code bos olamaz.", nameof(transactionCode));
        }

        Policy = $"{PolicyPrefix}{transactionCode.Trim().ToUpperInvariant()}";
    }
}
