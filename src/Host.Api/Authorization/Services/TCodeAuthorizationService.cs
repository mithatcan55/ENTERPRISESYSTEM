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
    public async Task<TCodeAccessResult> AuthorizeAsync(string transactionCode, int userId, int companyId, decimal? amount, CancellationToken cancellationToken)
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
                Value = x.Value,
                IsSatisfied = EvaluateCondition(x.FieldName, x.Operator, x.Value, amount)
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

    private static bool EvaluateCondition(string fieldName, string op, string value, decimal? amount)
    {
        if (!fieldName.Equals("amount", StringComparison.OrdinalIgnoreCase)
            && !fieldName.Equals("price", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (amount is null || !decimal.TryParse(value, out var threshold))
        {
            return false;
        }

        return op switch
        {
            "<" => amount.Value < threshold,
            "<=" => amount.Value <= threshold,
            ">" => amount.Value > threshold,
            ">=" => amount.Value >= threshold,
            "=" or "==" => amount.Value == threshold,
            "!=" => amount.Value != threshold,
            _ => false
        };
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
            Conditions = result.Conditions.Select(x => new { x.FieldName, x.Operator, x.Value, x.IsSatisfied })
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
