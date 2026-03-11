namespace Identity.Application.Contracts;

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
    public IReadOnlyList<PasswordPolicyPreviewSampleDto> Samples { get; set; } = Array.Empty<PasswordPolicyPreviewSampleDto>();
}

public sealed class PasswordPolicyPreviewSampleDto
{
    public string Password { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Email { get; set; }
}

public sealed record PasswordPolicyPreviewResultDto(
    bool IsValidConfiguration,
    IReadOnlyList<string> ValidationErrors,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<PasswordPolicySampleEvaluationDto> SampleEvaluations);

public sealed record PasswordPolicySampleEvaluationDto(
    string PasswordMasked,
    bool IsCompliant,
    IReadOnlyList<string> Errors);
