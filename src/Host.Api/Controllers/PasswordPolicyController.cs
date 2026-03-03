using Host.Api.Identity.Configuration;
using Host.Api.Identity.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/ops/security/password-policy")]
[Authorize(Roles = "SYS_ADMIN")]
public sealed class PasswordPolicyController(IOptions<PasswordPolicyOptions> passwordPolicyOptions) : ControllerBase
{
    /// <summary>
    /// Aktif password policy konfigürasyonunu getirir.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PasswordPolicySnapshotDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public ActionResult<PasswordPolicySnapshotDto> Get()
    {
        var options = passwordPolicyOptions.Value;

        var snapshot = new PasswordPolicySnapshotDto(
            options.MinLength,
            options.RequireUppercase,
            options.RequireLowercase,
            options.RequireDigit,
            options.RequireSpecialCharacter,
            options.HistoryCount,
            options.MinimumPasswordAgeMinutes);

        return Ok(snapshot);
    }

    /// <summary>
    /// Önerilen password policy değerlerini runtime'a yazmadan doğrular ve örnek şifrelerle simüle eder.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(PasswordPolicyPreviewResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public ActionResult<PasswordPolicyPreviewResult> Preview([FromBody] PasswordPolicyPreviewRequest request)
    {
        var validationErrors = ValidateConfiguration(request);
        var warnings = BuildWarnings(request);

        var evaluations = request.Samples
            .Select(sample =>
            {
                var errors = EvaluateSample(request, sample);
                return new PasswordPolicySampleEvaluation(
                    PasswordMasked: MaskPassword(sample.Password),
                    IsCompliant: errors.Count == 0,
                    Errors: errors);
            })
            .ToList();

        var result = new PasswordPolicyPreviewResult(
            IsValidConfiguration: validationErrors.Count == 0,
            ValidationErrors: validationErrors,
            Warnings: warnings,
            SampleEvaluations: evaluations);

        return Ok(result);
    }

    private static List<string> ValidateConfiguration(PasswordPolicyPreviewRequest request)
    {
        var errors = new List<string>();

        if (request.MinLength is < 8 or > 128)
        {
            errors.Add("MinLength 8 ile 128 arasında olmalıdır.");
        }

        if (request.HistoryCount is < 1 or > 24)
        {
            errors.Add("HistoryCount 1 ile 24 arasında olmalıdır.");
        }

        if (request.MinimumPasswordAgeMinutes is < 0 or > 1440)
        {
            errors.Add("MinimumPasswordAgeMinutes 0 ile 1440 arasında olmalıdır.");
        }

        if (!request.RequireUppercase && !request.RequireLowercase && !request.RequireDigit && !request.RequireSpecialCharacter)
        {
            errors.Add("En az bir complexity kuralı aktif olmalıdır (uppercase/lowercase/digit/special). ");
        }

        return errors;
    }

    private static List<string> BuildWarnings(PasswordPolicyPreviewRequest request)
    {
        var warnings = new List<string>();

        if (request.MinLength >= 20)
        {
            warnings.Add("MinLength değeri yüksek; kullanıcı deneyimi etkilenebilir.");
        }

        if (request.MinimumPasswordAgeMinutes >= 60)
        {
            warnings.Add("MinimumPasswordAgeMinutes yüksek; acil şifre değişimi senaryoları zorlaşabilir.");
        }

        if (request.HistoryCount >= 12)
        {
            warnings.Add("HistoryCount yüksek; kullanıcıların tekrar kullanabileceği parola havuzu daralır.");
        }

        warnings.Add("Preview sonucu runtime konfigürasyonunu değiştirmez.");
        warnings.Add("History/reuse lock simülasyonu örnek şifre düzeyindedir; kullanıcı bazlı geçmiş hash karşılaştırması yapılmaz.");

        return warnings;
    }

    private static List<string> EvaluateSample(PasswordPolicyPreviewRequest request, PasswordPolicyPreviewSample sample)
    {
        var errors = new List<string>();
        var password = sample.Password ?? string.Empty;

        if (password.Length < request.MinLength)
        {
            errors.Add($"Şifre en az {request.MinLength} karakter olmalıdır.");
        }

        if (request.RequireUppercase && !password.Any(char.IsUpper))
        {
            errors.Add("Şifre en az 1 büyük harf içermelidir.");
        }

        if (request.RequireLowercase && !password.Any(char.IsLower))
        {
            errors.Add("Şifre en az 1 küçük harf içermelidir.");
        }

        if (request.RequireDigit && !password.Any(char.IsDigit))
        {
            errors.Add("Şifre en az 1 rakam içermelidir.");
        }

        if (request.RequireSpecialCharacter && !password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            errors.Add("Şifre en az 1 özel karakter içermelidir.");
        }

        if (!string.IsNullOrWhiteSpace(sample.Username) && password.Contains(sample.Username, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Şifre kullanıcı adını içeremez.");
        }

        if (!string.IsNullOrWhiteSpace(sample.Email))
        {
            var localPart = sample.Email.Split('@', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(localPart) && password.Contains(localPart, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("Şifre email bilgisini içeremez.");
            }
        }

        return errors;
    }

    private static string MaskPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return string.Empty;
        }

        if (password.Length <= 2)
        {
            return new string('*', password.Length);
        }

        return $"{password[0]}{new string('*', password.Length - 2)}{password[^1]}";
    }
}
