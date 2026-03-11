using Application.Security;
using Authorization.Application.Contracts;
using Authorization.Application.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Authorization.Infrastructure.Services;

public sealed class TCodeAuthorizationService(
    AuthorizationDbContext authorizationDbContext,
    LogDbContext logDbContext,
    ICurrentUserContext currentUserContext,
    IHttpContextAccessor httpContextAccessor) : ITCodeAuthorizationService
{
    public async Task<TCodeAccessResult> AuthorizeAsync(
        string transactionCode,
        int userId,
        int companyId,
        IReadOnlyDictionary<string, string?> contextValues,
        string? requiredActionCode,
        bool denyOnUnsatisfiedConditions,
        CancellationToken cancellationToken)
    {
        var normalizedTCode = transactionCode.Trim().ToUpperInvariant();
        var normalizedRequiredActionCode = string.IsNullOrWhiteSpace(requiredActionCode)
            ? null
            : requiredActionCode.Trim().ToUpperInvariant();

        var page = await authorizationDbContext.SubModulePages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TransactionCode == normalizedTCode && !x.IsDeleted, cancellationToken);

        if (page is null)
        {
            return await DenyAsync(normalizedTCode, userId, 3, $"T-Code '{normalizedTCode}' icin eslesen sayfa bulunamadi.", normalizedRequiredActionCode, cancellationToken);
        }

        var subModule = await authorizationDbContext.SubModules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == page.SubModuleId && !x.IsDeleted, cancellationToken);

        if (subModule is null)
        {
            return await DenyAsync(normalizedTCode, userId, 2, $"T-Code '{normalizedTCode}' icin alt modul bilgisi bulunamadi.", normalizedRequiredActionCode, cancellationToken);
        }

        var module = await authorizationDbContext.Modules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == subModule.ModuleId && !x.IsDeleted, cancellationToken);

        if (module is null)
        {
            return await DenyAsync(normalizedTCode, userId, 1, $"T-Code '{normalizedTCode}' icin modul bilgisi bulunamadi.", normalizedRequiredActionCode, cancellationToken);
        }

        var level1Allowed = await authorizationDbContext.UserModulePermissions
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.ModuleId == module.Id && !x.IsDeleted, cancellationToken);

        if (!level1Allowed)
        {
            return await DenyAsync(normalizedTCode, userId, 1, $"Kullanicinin T-Code '{normalizedTCode}' icin modul erisim yetkisi yok.", normalizedRequiredActionCode, cancellationToken);
        }

        var level2Allowed = await authorizationDbContext.UserSubModulePermissions
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.SubModuleId == subModule.Id && !x.IsDeleted, cancellationToken);

        if (!level2Allowed)
        {
            return await DenyAsync(normalizedTCode, userId, 2, $"Kullanicinin T-Code '{normalizedTCode}' icin alt modul erisim yetkisi yok.", normalizedRequiredActionCode, cancellationToken);
        }

        var level3Allowed = await authorizationDbContext.UserPagePermissions
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.SubModulePageId == page.Id && !x.IsDeleted, cancellationToken);

        if (!level3Allowed)
        {
            return await DenyAsync(normalizedTCode, userId, 3, $"Kullanicinin T-Code '{normalizedTCode}' icin sayfa erisim yetkisi yok.", normalizedRequiredActionCode, cancellationToken);
        }

        var level4Allowed = await authorizationDbContext.UserCompanyPermissions
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.CompanyId == companyId && !x.IsDeleted, cancellationToken);

        if (!level4Allowed)
        {
            return await DenyAsync(normalizedTCode, userId, 4, $"Kullanicinin companyId '{companyId}' icin sirket kapsam yetkisi yok. T-Code: '{normalizedTCode}'.", normalizedRequiredActionCode, cancellationToken);
        }

        var actionPermissions = await authorizationDbContext.UserPageActionPermissions
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.SubModulePageId == page.Id && !x.IsDeleted)
            .ToListAsync(cancellationToken);

        var actions = actionPermissions
            .GroupBy(x => x.ActionCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Any(x => x.IsAllowed), StringComparer.OrdinalIgnoreCase);

        if (normalizedRequiredActionCode is not null && actionPermissions.Count > 0)
        {
            if (!actions.TryGetValue(normalizedRequiredActionCode, out var actionAllowed) || !actionAllowed)
            {
                return await DenyAsync(
                    normalizedTCode,
                    userId,
                    5,
                    $"Kullanicinin T-Code '{normalizedTCode}' icin '{normalizedRequiredActionCode}' aksiyon yetkisi yok.",
                    normalizedRequiredActionCode,
                    cancellationToken);
            }
        }

        var conditionPermissions = await authorizationDbContext.UserPageConditionPermissions
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.SubModulePageId == page.Id && x.IsActive && !x.IsDeleted)
            .ToListAsync(cancellationToken);

        var conditionResults = conditionPermissions
            .Select(x => new TCodeConditionResult
            {
                FieldName = x.FieldName,
                Operator = x.Operator,
                ExpectedValue = x.Value,
                ActualValue = ResolveActualValue(contextValues, x.FieldName),
                IsSatisfied = EvaluateCondition(ResolveActualValue(contextValues, x.FieldName), x.Operator, x.Value)
            })
            .ToList();

        var missingContextFields = conditionResults
            .Where(x => x.ActualValue is null)
            .Select(x => x.FieldName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var failedConditions = conditionResults
            .Where(x => x.ActualValue is not null && !x.IsSatisfied)
            .ToList();

        if (denyOnUnsatisfiedConditions && failedConditions.Count > 0)
        {
            var firstFailedCondition = failedConditions[0];

            return await DenyAsync(
                normalizedTCode,
                userId,
                6,
                $"Kullanicinin T-Code '{normalizedTCode}' icin kosul yetkisi saglanamadi. Alan: '{firstFailedCondition.FieldName}', operator: '{firstFailedCondition.Operator}', beklenen: '{firstFailedCondition.ExpectedValue}', gelen: '{firstFailedCondition.ActualValue}'.",
                normalizedRequiredActionCode,
                cancellationToken,
                conditionResults,
                missingContextFields);
        }

        bool? amountVisible = null;
        var amountConditions = conditionResults
            .Where(x => x.FieldName.Equals("amount", StringComparison.OrdinalIgnoreCase)
                     || x.FieldName.Equals("price", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (amountConditions.Count > 0)
        {
            amountVisible = amountConditions.All(x => x.IsSatisfied);
        }

        var allowResult = new TCodeAccessResult
        {
            TransactionCode = normalizedTCode,
            ModuleCode = module.Code,
            SubModuleCode = subModule.Code,
            PageCode = page.Code,
            RouteLink = page.RouteLink,
            IsAllowed = true,
            RequiredActionCode = normalizedRequiredActionCode,
            Actions = actions,
            Conditions = conditionResults,
            MissingContextFields = missingContextFields,
            AmountVisible = amountVisible
        };

        await LogSecurityEventAsync(allowResult, userId, true, null, cancellationToken);
        return allowResult;
    }

    private static string? ResolveActualValue(IReadOnlyDictionary<string, string?> contextValues, string fieldName)
    {
        if (contextValues.TryGetValue(fieldName, out var directValue))
        {
            return directValue;
        }

        var lowered = fieldName.ToLowerInvariant();
        if (contextValues.TryGetValue(lowered, out var loweredValue))
        {
            return loweredValue;
        }

        return null;
    }

    private static bool EvaluateCondition(string? actualValue, string op, string expectedValue)
    {
        if (actualValue is null)
        {
            return false;
        }

        var normalizedOperator = op.Trim().ToLowerInvariant();

        if (TryDecimal(actualValue, out var actualDecimal) && TryDecimal(expectedValue, out var expectedDecimal))
        {
            return CompareDecimal(actualDecimal, expectedDecimal, normalizedOperator);
        }

        if (TryDateTime(actualValue, out var actualDate) && TryDateTime(expectedValue, out var expectedDate))
        {
            return CompareDate(actualDate, expectedDate, normalizedOperator);
        }

        if (TryBool(actualValue, out var actualBool) && TryBool(expectedValue, out var expectedBool))
        {
            return normalizedOperator switch
            {
                "=" or "==" => actualBool == expectedBool,
                "!=" => actualBool != expectedBool,
                _ => false
            };
        }

        return CompareString(actualValue, expectedValue, normalizedOperator);
    }

    private static bool CompareDecimal(decimal actual, decimal expected, string op) => op switch
    {
        "<" => actual < expected,
        "<=" => actual <= expected,
        ">" => actual > expected,
        ">=" => actual >= expected,
        "=" or "==" => actual == expected,
        "!=" => actual != expected,
        _ => false
    };

    private static bool CompareDate(DateTime actual, DateTime expected, string op) => op switch
    {
        "<" => actual < expected,
        "<=" => actual <= expected,
        ">" => actual > expected,
        ">=" => actual >= expected,
        "=" or "==" => actual == expected,
        "!=" => actual != expected,
        _ => false
    };

    private static bool CompareString(string actual, string expected, string op) => op switch
    {
        "=" or "==" => string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
        "!=" => !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
        "contains" => actual.Contains(expected, StringComparison.OrdinalIgnoreCase),
        "startswith" => actual.StartsWith(expected, StringComparison.OrdinalIgnoreCase),
        "endswith" => actual.EndsWith(expected, StringComparison.OrdinalIgnoreCase),
        "in" => expected.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(x => string.Equals(x, actual, StringComparison.OrdinalIgnoreCase)),
        "notin" => expected.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .All(x => !string.Equals(x, actual, StringComparison.OrdinalIgnoreCase)),
        _ => false
    };

    private static bool TryDecimal(string value, out decimal parsed)
        => decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsed)
           || decimal.TryParse(value, out parsed);

    private static bool TryDateTime(string value, out DateTime parsed) => DateTime.TryParse(value, out parsed);

    private static bool TryBool(string value, out bool parsed)
    {
        if (bool.TryParse(value, out parsed))
        {
            return true;
        }

        if (value == "1")
        {
            parsed = true;
            return true;
        }

        if (value == "0")
        {
            parsed = false;
            return true;
        }

        return false;
    }

    private async Task<TCodeAccessResult> DenyAsync(
        string transactionCode,
        int userId,
        short level,
        string reason,
        string? requiredActionCode,
        CancellationToken cancellationToken,
        List<TCodeConditionResult>? conditions = null,
        List<string>? missingContextFields = null)
    {
        var denied = new TCodeAccessResult
        {
            TransactionCode = transactionCode,
            IsAllowed = false,
            DeniedAtLevel = level,
            DeniedReason = reason,
            RequiredActionCode = requiredActionCode,
            Conditions = conditions ?? [],
            MissingContextFields = missingContextFields ?? []
        };

        await LogSecurityEventAsync(denied, userId, false, reason, cancellationToken);
        return denied;
    }

    private async Task LogSecurityEventAsync(
        TCodeAccessResult result,
        int userId,
        bool isSuccessful,
        string? failureReason,
        CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var userIdentity = currentUserContext.TryGetActorIdentity(out var actorIdentity)
            ? actorIdentity
            : userId.ToString();

        var username = currentUserContext.TryGetUsername(out var currentUsername)
            ? currentUsername
            : userIdentity;

        var payload = JsonSerializer.Serialize(new
        {
            NumericUserId = userId,
            result.ModuleCode,
            result.SubModuleCode,
            result.PageCode,
            result.RouteLink,
            result.RequiredActionCode,
            result.Actions,
            result.AmountVisible,
            result.MissingContextFields,
            Conditions = result.Conditions.Select(x => new { x.FieldName, x.Operator, x.ExpectedValue, x.ActualValue, x.IsSatisfied })
        });

        var log = new SecurityEventLog
        {
            Timestamp = DateTimeOffset.UtcNow,
            EventType = "TCodeAccess",
            Severity = isSuccessful ? "Information" : "Warning",
            UserId = userIdentity,
            Username = username,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
            Resource = result.TransactionCode,
            Action = "Authorize",
            IsSuccessful = isSuccessful,
            FailureReason = failureReason,
            AdditionalData = payload
        };

        logDbContext.SecurityEventLogs.Add(log);
        await logDbContext.SaveChangesAsync(cancellationToken);
    }
}
