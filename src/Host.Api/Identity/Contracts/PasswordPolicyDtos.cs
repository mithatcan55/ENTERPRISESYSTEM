namespace Host.Api.Identity.Contracts;

public sealed record PasswordPolicySnapshotDto(
    int MinLength,
    bool RequireUppercase,
    bool RequireLowercase,
    bool RequireDigit,
    bool RequireSpecialCharacter,
    int HistoryCount,
    int MinimumPasswordAgeMinutes);

public sealed class PasswordPolicyPreviewRequest
{
    public int MinLength { get; set; }
    public bool RequireUppercase { get; set; }
    public bool RequireLowercase { get; set; }
    public bool RequireDigit { get; set; }
    public bool RequireSpecialCharacter { get; set; }
    public int HistoryCount { get; set; }
    public int MinimumPasswordAgeMinutes { get; set; }
    public IReadOnlyList<PasswordPolicyPreviewSample> Samples { get; set; } = Array.Empty<PasswordPolicyPreviewSample>();
}

public sealed class PasswordPolicyPreviewSample
{
    public string Password { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Email { get; set; }
}

public sealed record PasswordPolicyPreviewResult(
    bool IsValidConfiguration,
    IReadOnlyList<string> ValidationErrors,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<PasswordPolicySampleEvaluation> SampleEvaluations);

public sealed record PasswordPolicySampleEvaluation(
    string PasswordMasked,
    bool IsCompliant,
    IReadOnlyList<string> Errors);
