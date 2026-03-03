using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.CodeAnalysis;

namespace Host.Api.Security.Authorization;

public sealed class TCodeRequirement(string transactionCode) : IAuthorizationRequirement
{
    public string TransactionCode { get; } = transactionCode;
}

/// <summary>
/// Dinamik T-Code policy kullanımı için attribute.
/// Örn: [TCodeAuthorize("SYS01")]
/// </summary>
public sealed class TCodeAuthorizeAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "TCode:";

    public TCodeAuthorizeAttribute([DisallowNull] string transactionCode)
    {
        if (string.IsNullOrWhiteSpace(transactionCode))
        {
            throw new ArgumentException("Transaction code boş olamaz.", nameof(transactionCode));
        }

        Policy = $"{PolicyPrefix}{transactionCode.Trim().ToUpperInvariant()}";
    }
}
