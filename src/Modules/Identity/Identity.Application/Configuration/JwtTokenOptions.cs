namespace Identity.Application.Configuration;

public sealed class JwtTokenOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "EnterpriseSystem";
    public string Audience { get; set; } = "EnterpriseSystem.Client";
    public string SigningKey { get; set; } = "CHANGE_ME_SUPER_SECRET_KEY_32_CHARS_MINIMUM";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
