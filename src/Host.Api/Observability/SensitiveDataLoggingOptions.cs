namespace Host.Api.Observability;

public sealed class SensitiveDataLoggingOptions
{
    public const string SectionName = "Observability:SensitiveDataLogging";

    public List<string> RedactedHeaders { get; init; } =
    [
        "Authorization",
        "Cookie",
        "Set-Cookie",
        "X-Api-Key"
    ];

    public List<string> RedactedJsonFields { get; init; } =
    [
        "password",
        "currentPassword",
        "newPassword",
        "refreshToken",
        "accessToken",
        "token",
        "tokenHash",
        "clientSecret",
        "secret",
        "signingKey"
    ];

    public List<string> ExcludedPaths { get; init; } =
    [
        "/api/auth/login",
        "/api/auth/refresh",
        "/api/auth/change-password"
    ];

    public string MaskValue { get; init; } = "***REDACTED***";
}
