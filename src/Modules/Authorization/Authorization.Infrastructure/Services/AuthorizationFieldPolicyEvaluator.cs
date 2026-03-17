using System.Globalization;
using Application.Security;
using Authorization.Application.Contracts;
using Authorization.Application.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Infrastructure.Services;

/// <summary>
/// Dynamic field policy kurallarini kullanici baglami ve entity field degerleri ile birlikte degerlendirir.
/// Bu servis UI'nin kolon, form ve filtre davranisini veriden uretmesini saglar.
/// </summary>
public sealed class AuthorizationFieldPolicyEvaluator(
    AuthorizationDbContext authorizationDbContext,
    ICurrentUserContext currentUserContext,
    IHttpContextAccessor httpContextAccessor) : IAuthorizationFieldPolicyEvaluator
{
    public async Task<IReadOnlyList<AuthorizationFieldPolicyDecisionDto>> EvaluateAsync(
        EvaluateAuthorizationFieldPolicyRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.EntityName))
        {
            return [];
        }

        var normalizedEntityName = NormalizeKey(request.EntityName);
        var normalizedSurface = string.IsNullOrWhiteSpace(request.Surface) ? "ANY" : NormalizeEnumLike(request.Surface);
        var normalizedFieldValues = request.FieldValues
            .ToDictionary(x => NormalizeKey(x.Key), x => x.Value, StringComparer.OrdinalIgnoreCase);
        var definitions = await authorizationDbContext.AuthorizationFieldDefinitions
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive && x.EntityName == normalizedEntityName)
            .OrderBy(x => x.FieldName)
            .ToListAsync(cancellationToken);

        if (definitions.Count == 0)
        {
            return [];
        }

        var rules = await authorizationDbContext.AuthorizationFieldPolicies
            .AsNoTracking()
            .Where(x => !x.IsDeleted
                        && x.IsActive
                        && x.EntityName == normalizedEntityName
                        && (x.Surface == "ANY" || x.Surface == normalizedSurface))
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var decisions = new List<AuthorizationFieldPolicyDecisionDto>(definitions.Count);
        foreach (var definition in definitions)
        {
            var state = new EvaluationState(
                definition.DefaultVisible,
                definition.DefaultEditable,
                definition.DefaultFilterable,
                definition.DefaultExportable,
                false,
                null);

            foreach (var rule in rules.Where(x => x.FieldName == definition.FieldName))
            {
                if (!MatchesTarget(rule) || !MatchesCondition(rule, normalizedFieldValues))
                {
                    continue;
                }

                ApplyEffect(state, rule);
            }

            decisions.Add(new AuthorizationFieldPolicyDecisionDto(
                normalizedEntityName,
                definition.FieldName,
                definition.DisplayName,
                state.Visible,
                state.Editable,
                state.Filterable,
                state.Exportable,
                state.Masked,
                state.MaskingMode));
        }

        return decisions;
    }

    private bool MatchesTarget(AuthorizationFieldPolicy rule)
    {
        var targetType = NormalizeEnumLike(rule.TargetType);
        if (targetType == "ANY")
        {
            return true;
        }

        if (targetType == "USER")
        {
            return currentUserContext.TryGetUserId(out var userId)
                   && string.Equals(rule.TargetKey, userId.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);
        }

        if (targetType == "ROLE")
        {
            return !string.IsNullOrWhiteSpace(rule.TargetKey) && currentUserContext.IsInRole(rule.TargetKey);
        }

        if (targetType == "PERMISSION")
        {
            var claims = httpContextAccessor.HttpContext?.User.Claims
                .Where(x => string.Equals(x.Type, SecurityClaimTypes.Permission, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return claims?.Contains(rule.TargetKey ?? string.Empty) == true;
        }

        return false;
    }

    private static bool MatchesCondition(AuthorizationFieldPolicy rule, IReadOnlyDictionary<string, string?> fieldValues)
    {
        var op = NormalizeEnumLike(rule.ConditionOperator);
        if (op == "ALWAYS")
        {
            return true;
        }

        var sourceField = string.IsNullOrWhiteSpace(rule.ConditionFieldName) ? rule.FieldName : rule.ConditionFieldName;
        fieldValues.TryGetValue(sourceField, out var rawValue);
        var compareValue = rule.CompareValue;

        return op switch
        {
            "EQ" => string.Equals(rawValue, compareValue, StringComparison.OrdinalIgnoreCase),
            "NE" => !string.Equals(rawValue, compareValue, StringComparison.OrdinalIgnoreCase),
            "CONTAINS" => rawValue?.Contains(compareValue ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true,
            "STARTS_WITH" => rawValue?.StartsWith(compareValue ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true,
            "ENDS_WITH" => rawValue?.EndsWith(compareValue ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true,
            "IS_NULL" => string.IsNullOrWhiteSpace(rawValue),
            "NOT_NULL" => !string.IsNullOrWhiteSpace(rawValue),
            "GT" => CompareNumbers(rawValue, compareValue) > 0,
            "GTE" => CompareNumbers(rawValue, compareValue) >= 0,
            "LT" => CompareNumbers(rawValue, compareValue) < 0,
            "LTE" => CompareNumbers(rawValue, compareValue) <= 0,
            _ => false
        };
    }

    private static int CompareNumbers(string? left, string? right)
    {
        if (!TryParseDecimal(left, out var leftValue) || !TryParseDecimal(right, out var rightValue))
        {
            return int.MinValue;
        }

        return decimal.Compare(leftValue, rightValue);
    }

    private static bool TryParseDecimal(string? value, out decimal result)
    {
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
        {
            return true;
        }

        return decimal.TryParse(value, NumberStyles.Any, CultureInfo.GetCultureInfo("tr-TR"), out result);
    }

    private static void ApplyEffect(EvaluationState state, AuthorizationFieldPolicy rule)
    {
        var effect = NormalizeEnumLike(rule.Effect);
        switch (effect)
        {
            case "HIDE":
                state.Visible = false;
                state.Editable = false;
                break;
            case "SHOW":
                state.Visible = true;
                state.Masked = false;
                state.MaskingMode = null;
                break;
            case "READONLY":
                state.Visible = true;
                state.Editable = false;
                break;
            case "EDITABLE":
                state.Visible = true;
                state.Editable = true;
                break;
            case "MASK":
                state.Visible = true;
                state.Masked = true;
                state.MaskingMode = string.IsNullOrWhiteSpace(rule.MaskingMode) ? "FULL" : NormalizeEnumLike(rule.MaskingMode!);
                break;
            case "SHOW_FILTER":
                state.Filterable = true;
                break;
            case "HIDE_FILTER":
                state.Filterable = false;
                break;
            case "SHOW_EXPORT":
                state.Exportable = true;
                break;
            case "HIDE_EXPORT":
                state.Exportable = false;
                break;
        }
    }

    private static string NormalizeKey(string value)
        => value.Trim().Replace(" ", string.Empty).ToUpperInvariant();

    private static string NormalizeEnumLike(string value)
        => value.Trim().Replace(" ", "_").ToUpperInvariant();

    private sealed class EvaluationState(
        bool visible,
        bool editable,
        bool filterable,
        bool exportable,
        bool masked,
        string? maskingMode)
    {
        public bool Visible { get; set; } = visible;
        public bool Editable { get; set; } = editable;
        public bool Filterable { get; set; } = filterable;
        public bool Exportable { get; set; } = exportable;
        public bool Masked { get; set; } = masked;
        public string? MaskingMode { get; set; } = maskingMode;
    }
}
