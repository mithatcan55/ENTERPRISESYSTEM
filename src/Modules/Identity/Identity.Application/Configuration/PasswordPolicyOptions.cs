using System.ComponentModel.DataAnnotations;

namespace Identity.Application.Configuration;

public sealed class PasswordPolicyOptions
{
    public const string SectionName = "PasswordPolicy";

    [Range(8, 128)]
    public int MinLength { get; set; } = 12;

    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialCharacter { get; set; } = true;

    [Range(1, 24)]
    public int HistoryCount { get; set; } = 5;

    [Range(0, 1440)]
    public int MinimumPasswordAgeMinutes { get; set; } = 5;

    /// <summary>0 = never expires</summary>
    [Range(0, 365)]
    public int PasswordExpiryDays { get; set; } = 90;

    /// <summary>Show warning this many days before expiry</summary>
    [Range(1, 30)]
    public int ExpiryWarningDays { get; set; } = 14;
}
