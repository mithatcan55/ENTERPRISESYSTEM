using System.Text.RegularExpressions;

namespace Host.Api.Configuration;

public static partial class StartupSecurityValidator
{
    public static void EnsureNoPlaceholderSecrets(IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsEnvironment("Testing"))
        {
            return;
        }

        var allowWeakDevelopmentSecrets = environment.IsDevelopment();

        ValidateConnectionString(configuration.GetConnectionString("BusinessDb"), "ConnectionStrings:BusinessDb", allowWeakDevelopmentSecrets);
        ValidateConnectionString(configuration.GetConnectionString("LogDb"), "ConnectionStrings:LogDb", allowWeakDevelopmentSecrets);
        ValidateJwtSigningKey(configuration["Jwt:SigningKey"], allowWeakDevelopmentSecrets);
        ValidateBootstrapPassword(configuration["BootstrapAdmin:Password"], allowWeakDevelopmentSecrets);
    }

    private static void ValidateConnectionString(string? connectionString, string key, bool allowWeakDevelopmentSecrets)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"{key} zorunludur.");
        }

        var passwordMatch = ConnectionStringPasswordRegex().Match(connectionString);
        if (!passwordMatch.Success)
        {
            throw new InvalidOperationException($"{key} i\u00e7in Password alan\u0131 zorunludur.");
        }

        var password = passwordMatch.Groups["password"].Value;
        if (IsPlaceholder(password, allowWeakDevelopmentSecrets))
        {
            throw new InvalidOperationException($"{key} i\u00e7inde placeholder/de\u011ferlendirme d\u0131\u015f\u0131 bir parola bulundu. Ger\u00e7ek secret environment variable veya user-secrets ile verilmelidir.");
        }
    }

    private static void ValidateJwtSigningKey(string? signingKey, bool allowWeakDevelopmentSecrets)
    {
        if (string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException("Jwt:SigningKey zorunludur.");
        }

        if (IsPlaceholder(signingKey, allowWeakDevelopmentSecrets))
        {
            throw new InvalidOperationException("Jwt:SigningKey placeholder olarak b\u0131rak\u0131lm\u0131\u015f. Ger\u00e7ek secret environment variable veya user-secrets ile verilmelidir.");
        }
    }

    private static void ValidateBootstrapPassword(string? password, bool allowWeakDevelopmentSecrets)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("BootstrapAdmin:Password zorunludur.");
        }

        if (IsPlaceholder(password, allowWeakDevelopmentSecrets))
        {
            throw new InvalidOperationException("BootstrapAdmin:Password placeholder olarak b\u0131rak\u0131lm\u0131\u015f. Ger\u00e7ek de\u011fer environment variable veya user-secrets ile verilmelidir.");
        }
    }

    private static bool IsPlaceholder(string value, bool allowWeakDevelopmentSecrets)
    {
        return value.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase)
               || value.Contains("DEV_ONLY", StringComparison.OrdinalIgnoreCase)
               || (!allowWeakDevelopmentSecrets && value == "123456");
    }

    [GeneratedRegex(@"(?:^|;)Password=(?<password>[^;]+)", RegexOptions.IgnoreCase)]
    private static partial Regex ConnectionStringPasswordRegex();
}
