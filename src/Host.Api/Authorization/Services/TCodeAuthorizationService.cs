using Host.Api.Authorization.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Host.Api.Authorization.Services;

/// <summary>
/// 6 seviye yetki modeline göre T-Code erişimi doğrular.
/// </summary>
public sealed class TCodeAuthorizationService(BusinessDbContext businessDbContext) : ITCodeAuthorizationService
{
    public async Task<TCodeAccessResult> AuthorizeAsync(string transactionCode, int userId, int companyId, decimal? amount, CancellationToken cancellationToken)
    {
        var normalizedTCode = transactionCode.Trim().ToUpperInvariant();

        var page = await businessDbContext.SubModulePages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TransactionCode == normalizedTCode && !x.IsDeleted, cancellationToken);

        if (page is null)
        {
            return Denied(normalizedTCode, 3, "T-Code için eşleşen sayfa bulunamadı.");
        }

        var subModule = await businessDbContext.SubModules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == page.SubModuleId && !x.IsDeleted, cancellationToken);

        if (subModule is null)
        {
            return Denied(normalizedTCode, 2, "T-Code için alt modül bilgisi bulunamadı.");
        }

        var module = await businessDbContext.Modules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == subModule.ModuleId && !x.IsDeleted, cancellationToken);

        if (module is null)
        {
            return Denied(normalizedTCode, 1, "T-Code için modül bilgisi bulunamadı.");
        }

        var level1Allowed = await businessDbContext.UserModulePermissions
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.ModuleId == module.Id && !x.IsDeleted, cancellationToken);

        if (!level1Allowed)
        {
            return Denied(normalizedTCode, 1, "Kullanıcının modül erişim yetkisi yok.");
        }

        var level2Allowed = await businessDbContext.UserSubModulePermissions
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.SubModuleId == subModule.Id && !x.IsDeleted, cancellationToken);

        if (!level2Allowed)
        {
            return Denied(normalizedTCode, 2, "Kullanıcının alt modül erişim yetkisi yok.");
        }

        var level3Allowed = await businessDbContext.UserPagePermissions
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.SubModulePageId == page.Id && !x.IsDeleted, cancellationToken);

        if (!level3Allowed)
        {
            return Denied(normalizedTCode, 3, "Kullanıcının sayfa/T-Code erişim yetkisi yok.");
        }

        var level4Allowed = await businessDbContext.UserCompanyPermissions
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.CompanyId == companyId && !x.IsDeleted, cancellationToken);

        if (!level4Allowed)
        {
            return Denied(normalizedTCode, 4, "Kullanıcının şirket kapsam yetkisi yok.");
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

        return new TCodeAccessResult
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

    private static TCodeAccessResult Denied(string transactionCode, short level, string reason)
    {
        return new TCodeAccessResult
        {
            TransactionCode = transactionCode,
            IsAllowed = false,
            DeniedAtLevel = level,
            DeniedReason = reason
        };
    }
}
