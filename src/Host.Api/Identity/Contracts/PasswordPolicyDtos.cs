namespace Host.Api.Identity.Contracts;

public sealed record PasswordPolicySnapshotDto(
    int MinLength,
    bool RequireUppercase,
    bool RequireLowercase,
    bool RequireDigit,
    bool RequireSpecialCharacter,
    int HistoryCount,
    int MinimumPasswordAgeMinutes);
