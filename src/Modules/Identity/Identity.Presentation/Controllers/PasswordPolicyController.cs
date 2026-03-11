using Identity.Application.Configuration;
using Identity.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Identity.Presentation.Controllers;

[ApiController]
[Route("api/ops/security/password-policy")]
[Authorize(Roles = "SYS_ADMIN")]
public sealed class PasswordPolicyController(IOptions<PasswordPolicyOptions> passwordPolicyOptions) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PasswordPolicySnapshotDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public ActionResult<PasswordPolicySnapshotDto> Get()
    {
        // Snapshot endpoint'i runtime config'i degistirmez.
        // Amaci o an sistemin hangi policy ile calistigini gostermektir.
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

    [HttpPut]
    [ProducesResponseType(typeof(PasswordPolicyPreviewResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public ActionResult<PasswordPolicyPreviewResultDto> Preview([FromBody] PasswordPolicyPreviewRequest request)
    {
        // Preview endpoint'i "config yazar" endpoint degildir.
        // Yalnizca yeni policy onerisi verilirse nasil davranacagini simule eder.
        var validationErrors = ValidateConfiguration(request);
        var warnings = BuildWarnings(request);

        var evaluations = request.Samples
            .Select(sample =>
            {
                var errors = EvaluateSample(request, sample);
                return new PasswordPolicySampleEvaluationDto(
                    PasswordMasked: MaskPassword(sample.Password),
                    IsCompliant: errors.Count == 0,
                    Errors: errors);
            })
            .ToList();

        var result = new PasswordPolicyPreviewResultDto(
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
            errors.Add("MinLength 8 ile 128 arasinda olmalidir.");
        }

        if (request.HistoryCount is < 1 or > 24)
        {
            errors.Add("HistoryCount 1 ile 24 arasinda olmalidir.");
        }

        if (request.MinimumPasswordAgeMinutes is < 0 or > 1440)
        {
            errors.Add("MinimumPasswordAgeMinutes 0 ile 1440 arasinda olmalidir.");
        }

        if (!request.RequireUppercase && !request.RequireLowercase && !request.RequireDigit && !request.RequireSpecialCharacter)
        {
            errors.Add("En az bir complexity kurali aktif olmalidir (uppercase/lowercase/digit/special).");
        }

        return errors;
    }

    private static List<string> BuildWarnings(PasswordPolicyPreviewRequest request)
    {
        var warnings = new List<string>();

        if (request.MinLength >= 20)
        {
            warnings.Add("MinLength degeri yuksek; kullanici deneyimi etkilenebilir.");
        }

        if (request.MinimumPasswordAgeMinutes >= 60)
        {
            warnings.Add("MinimumPasswordAgeMinutes yuksek; acil sifre degisimi senaryolari zorlasabilir.");
        }

        if (request.HistoryCount >= 12)
        {
            warnings.Add("HistoryCount yuksek; kullanicilarin tekrar kullanabilecegi parola havuzu daralir.");
        }

        warnings.Add("Preview sonucu runtime konfigurasyonunu degistirmez.");
        warnings.Add("History/reuse lock simulasyonu ornek sifre duzeyindedir; kullanici bazli gecmis hash karsilastirmasi yapilmaz.");

        return warnings;
    }

    private static List<string> EvaluateSample(PasswordPolicyPreviewRequest request, PasswordPolicyPreviewSampleDto sample)
    {
        // Bu metod hash tabanli gecmis karsilastirmasi yapmaz.
        // Sadece policy'nin ornek sifreler uzerindeki davranisini gosterir.
        var errors = new List<string>();
        var password = sample.Password ?? string.Empty;

        if (password.Length < request.MinLength)
        {
            errors.Add($"Sifre en az {request.MinLength} karakter olmalidir.");
        }

        if (request.RequireUppercase && !password.Any(char.IsUpper))
        {
            errors.Add("Sifre en az 1 buyuk harf icermelidir.");
        }

        if (request.RequireLowercase && !password.Any(char.IsLower))
        {
            errors.Add("Sifre en az 1 kucuk harf icermelidir.");
        }

        if (request.RequireDigit && !password.Any(char.IsDigit))
        {
            errors.Add("Sifre en az 1 rakam icermelidir.");
        }

        if (request.RequireSpecialCharacter && !password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            errors.Add("Sifre en az 1 ozel karakter icermelidir.");
        }

        if (!string.IsNullOrWhiteSpace(sample.Username) && password.Contains(sample.Username, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Sifre kullanici adini iceremez.");
        }

        if (!string.IsNullOrWhiteSpace(sample.Email))
        {
            var localPart = sample.Email.Split('@', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(localPart) && password.Contains(localPart, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("Sifre email bilgisini iceremez.");
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
