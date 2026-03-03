using Host.Api.Authorization.Models;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Host.Api.Authorization.Services;

/// <summary>
/// 6 seviye yetki modeline göre T-Code erişimi doğrular.
/// </summary>
public sealed class TCodeAuthorizationService(
    BusinessDbContext businessDbContext,
    LogDbContext logDbContext,
    IHttpContextAccessor httpContextAccessor) : ITCodeAuthorizationService
{
    public async Task<TCodeAccessResult> AuthorizeAsync(
        string transactionCode,
        int userId,
        int companyId,
        IReadOnlyDictionary<string, string?> contextValues,
        CancellationToken cancellationToken)
    {
        var normalizedTCode = transactionCode.Trim().ToUpperInvariant();

        var page = await businessDbContext.SubModulePages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TransactionCode == normalizedTCode && !x.IsDeleted, cancellationToken);

        if (page is null)
        {
            return await DenyAsync(normalizedTCode, userId, 3, "T-Code için eşleşen sayfa bulunamadı.", cancellationToken);
        }

        var subModule = await businessDbContext.SubModules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == page.SubModuleId && !x.IsDeleted, cancellationToken);

        if (subModule is null)
        {
            return await DenyAsync(normalizedTCode, userId, 2, "T-Code için alt modül bilgisi bulunamadı.", cancellationToken);
        }

        var module = await businessDbContext.Modules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == subModule.ModuleId && !x.IsDeleted, cancellationToken);

        if (module is null)
        {
            return await DenyAsync(normalizedTCode, userId, 1, "T-Code için modül bilgisi bulunamadı.", cancellationToken);
        }

        var level1Allowed = await businessDbContext.UserModulePermissions
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.ModuleId == module.Id && !x.IsDeleted, cancellationToken);

        if (!level1Allowed)
        {
            return await DenyAsync(normalizedTCode, userId, 1, "Kullanıcının modül erişim yetkisi yok.", cancellationToken);
        }

        var level2Allowed = await businessDbContext.UserSubModulePermissions
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.SubModuleId == subModule.Id && !x.IsDeleted, cancellationToken);

        if (!level2Allowed)
        {
            return await DenyAsync(normalizedTCode, userId, 2, "Kullanıcının alt modül erişim yetkisi yok.", cancellationToken);
        }

        var level3Allowed = await businessDbContext.UserPagePermissions
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.SubModulePageId == page.Id && !x.IsDeleted, cancellationToken);

        if (!level3Allowed)
        {
            return await DenyAsync(normalizedTCode, userId, 3, "Kullanıcının sayfa/T-Code erişim yetkisi yok.", cancellationToken);
        }

        var level4Allowed = await businessDbContext.UserCompanyPermissions
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.CompanyId == companyId && !x.IsDeleted, cancellationToken);

        if (!level4Allowed)
        {
            return await DenyAsync(normalizedTCode, userId, 4, "Kullanıcının şirket kapsam yetkisi yok.", cancellationToken);
        }

        var actionPermissions = await businessDbContext.UserPageActionPermissions
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.SubModulePageId == page.Id && !x.IsDeleted)
            .ToListAsync(cancellationToken);

        var actions = actionPermissions
            .GroupBy(x => x.ActionCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Any(x => x.IsAllowed), StringComparer.OrdinalIgnoreCase);

        var conditionPermissions = await businessDbContext.UserPageConditionPermissions
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
            Actions = actions,
            Conditions = conditionResults,
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

    private static bool CompareDecimal(decimal actual, decimal expected, string op)
    {
        return op switch
        {
            "<" => actual < expected,
            "<=" => actual <= expected,
            ">" => actual > expected,
            ">=" => actual >= expected,
            "=" or "==" => actual == expected,
            "!=" => actual != expected,
            _ => false
        };
    }

    private static bool CompareDate(DateTime actual, DateTime expected, string op)
    {
        return op switch
        {
            "<" => actual < expected,
            "<=" => actual <= expected,
            ">" => actual > expected,
            ">=" => actual >= expected,
            "=" or "==" => actual == expected,
            "!=" => actual != expected,
            _ => false
        };
    }

    private static bool CompareString(string actual, string expected, string op)
    {
        return op switch
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
    }

    private static bool TryDecimal(string value, out decimal parsed)
    {
        return decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsed)
               || decimal.TryParse(value, out parsed);
    }

    private static bool TryDateTime(string value, out DateTime parsed)
    {
        return DateTime.TryParse(value, out parsed);
    }

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

    private async Task<TCodeAccessResult> DenyAsync(string transactionCode, int userId, short level, string reason, CancellationToken cancellationToken)
    {
        var denied = new TCodeAccessResult
        {
            TransactionCode = transactionCode,
            IsAllowed = false,
            DeniedAtLevel = level,
            DeniedReason = reason
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
        var payload = JsonSerializer.Serialize(new
        {
            result.ModuleCode,
            result.SubModuleCode,
            result.PageCode,
            result.RouteLink,
            result.Actions,
            result.AmountVisible,
            Conditions = result.Conditions.Select(x => new { x.FieldName, x.Operator, x.ExpectedValue, x.ActualValue, x.IsSatisfied })
        });

        var log = new SecurityEventLog
        {
            Timestamp = DateTimeOffset.UtcNow,
            EventType = "TCodeAccess",
            Severity = isSuccessful ? "Information" : "Warning",
            UserId = userId.ToString(),
            Username = httpContext?.User?.Identity?.Name,
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
