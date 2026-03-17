using Application.Exceptions;
using Authorization.Application.Contracts;
using Authorization.Application.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Infrastructure.Services;

/// <summary>
/// Dynamic field policy tanimlarini DB uzerinden yoneten servis.
/// Bu servis sayesinde yeni alan ve kural eklemek icin kod degisikligi zorunlu olmaz.
/// </summary>
public sealed class AuthorizationFieldPolicyAdminService(
    AuthorizationDbContext authorizationDbContext) : IAuthorizationFieldPolicyAdminService
{
    public async Task<IReadOnlyList<AuthorizationFieldDefinitionDto>> ListDefinitionsAsync(string? entityName, CancellationToken cancellationToken)
    {
        var query = authorizationDbContext.AuthorizationFieldDefinitions.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            var normalizedEntityName = NormalizeKey(entityName);
            query = query.Where(x => x.EntityName == normalizedEntityName);
        }

        var items = await query
            .OrderBy(x => x.EntityName)
            .ThenBy(x => x.FieldName)
            .ToListAsync(cancellationToken);

        return items.Select(MapDefinition).ToList();
    }

    public async Task<AuthorizationFieldDefinitionDto> UpsertDefinitionAsync(UpsertAuthorizationFieldDefinitionRequest request, CancellationToken cancellationToken)
    {
        ValidateDefinitionRequest(request);

        var normalizedEntityName = NormalizeKey(request.EntityName);
        var normalizedFieldName = NormalizeKey(request.FieldName);
        var normalizedSurfaces = NormalizeSurfaces(request.AllowedSurfaces);

        var duplicateExists = await authorizationDbContext.AuthorizationFieldDefinitions
            .AnyAsync(
                x => x.Id != (request.Id ?? 0)
                     && !x.IsDeleted
                     && x.EntityName == normalizedEntityName
                     && x.FieldName == normalizedFieldName,
                cancellationToken);

        if (duplicateExists)
        {
            throw new ValidationAppException(
                "Field definition dogrulamasi basarisiz.",
                new Dictionary<string, string[]>
                {
                    ["fieldName"] = [$"{normalizedEntityName}.{normalizedFieldName} icin kayit zaten var."]
                });
        }

        AuthorizationFieldDefinition entity;
        if (request.Id.HasValue)
        {
            entity = await authorizationDbContext.AuthorizationFieldDefinitions
                .FirstOrDefaultAsync(x => x.Id == request.Id.Value && !x.IsDeleted, cancellationToken)
                ?? throw new NotFoundAppException($"Field definition bulunamadi. id={request.Id.Value}");
        }
        else
        {
            entity = new AuthorizationFieldDefinition();
            authorizationDbContext.AuthorizationFieldDefinitions.Add(entity);
        }

        entity.EntityName = normalizedEntityName;
        entity.FieldName = normalizedFieldName;
        entity.DisplayName = request.DisplayName.Trim();
        entity.DataType = NormalizeEnumLike(request.DataType);
        entity.AllowedSurfaces = normalizedSurfaces;
        entity.DefaultVisible = request.DefaultVisible;
        entity.DefaultEditable = request.DefaultEditable;
        entity.DefaultFilterable = request.DefaultFilterable;
        entity.DefaultExportable = request.DefaultExportable;
        entity.IsActive = request.IsActive;
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        await authorizationDbContext.SaveChangesAsync(cancellationToken);
        return MapDefinition(entity);
    }

    public async Task DeleteDefinitionAsync(int definitionId, CancellationToken cancellationToken)
    {
        var entity = await authorizationDbContext.AuthorizationFieldDefinitions
            .FirstOrDefaultAsync(x => x.Id == definitionId && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundAppException($"Field definition bulunamadi. id={definitionId}");

        authorizationDbContext.AuthorizationFieldDefinitions.Remove(entity);
        await authorizationDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuthorizationFieldPolicyDto>> ListPoliciesAsync(string? entityName, string? fieldName, CancellationToken cancellationToken)
    {
        var query = authorizationDbContext.AuthorizationFieldPolicies.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            var normalizedEntityName = NormalizeKey(entityName);
            query = query.Where(x => x.EntityName == normalizedEntityName);
        }

        if (!string.IsNullOrWhiteSpace(fieldName))
        {
            var normalizedFieldName = NormalizeKey(fieldName);
            query = query.Where(x => x.FieldName == normalizedFieldName);
        }

        var items = await query
            .OrderBy(x => x.EntityName)
            .ThenBy(x => x.FieldName)
            .ThenBy(x => x.Priority)
            .ToListAsync(cancellationToken);

        return items.Select(MapPolicy).ToList();
    }

    public async Task<AuthorizationFieldPolicyDto> UpsertPolicyAsync(UpsertAuthorizationFieldPolicyRequest request, CancellationToken cancellationToken)
    {
        ValidatePolicyRequest(request);

        var normalizedEntityName = NormalizeKey(request.EntityName);
        var normalizedFieldName = NormalizeKey(request.FieldName);
        var normalizedSurface = NormalizeEnumLike(request.Surface);
        var normalizedTargetType = NormalizeEnumLike(request.TargetType);
        var normalizedEffect = NormalizeEnumLike(request.Effect);
        var normalizedOperator = NormalizeEnumLike(request.ConditionOperator);
        var normalizedMaskingMode = string.IsNullOrWhiteSpace(request.MaskingMode) ? null : NormalizeEnumLike(request.MaskingMode);
        var normalizedConditionField = string.IsNullOrWhiteSpace(request.ConditionFieldName) ? null : NormalizeKey(request.ConditionFieldName);
        var normalizedTargetKey = string.IsNullOrWhiteSpace(request.TargetKey) ? null : request.TargetKey.Trim().ToUpperInvariant();

        var definitionExists = await authorizationDbContext.AuthorizationFieldDefinitions
            .AsNoTracking()
            .AnyAsync(
                x => !x.IsDeleted
                     && x.EntityName == normalizedEntityName
                     && x.FieldName == normalizedFieldName,
                cancellationToken);

        if (!definitionExists)
        {
            throw new ValidationAppException(
                "Field policy dogrulamasi basarisiz.",
                new Dictionary<string, string[]>
                {
                    ["fieldName"] = [$"{normalizedEntityName}.{normalizedFieldName} icin once field definition tanimlanmalidir."]
                });
        }

        AuthorizationFieldPolicy entity;
        if (request.Id.HasValue)
        {
            entity = await authorizationDbContext.AuthorizationFieldPolicies
                .FirstOrDefaultAsync(x => x.Id == request.Id.Value && !x.IsDeleted, cancellationToken)
                ?? throw new NotFoundAppException($"Field policy bulunamadi. id={request.Id.Value}");
        }
        else
        {
            entity = new AuthorizationFieldPolicy();
            authorizationDbContext.AuthorizationFieldPolicies.Add(entity);
        }

        entity.Name = request.Name.Trim();
        entity.EntityName = normalizedEntityName;
        entity.FieldName = normalizedFieldName;
        entity.Surface = normalizedSurface;
        entity.TargetType = normalizedTargetType;
        entity.TargetKey = normalizedTargetKey;
        entity.Effect = normalizedEffect;
        entity.ConditionFieldName = normalizedConditionField;
        entity.ConditionOperator = normalizedOperator;
        entity.CompareValue = string.IsNullOrWhiteSpace(request.CompareValue) ? null : request.CompareValue.Trim();
        entity.MaskingMode = normalizedMaskingMode;
        entity.Priority = request.Priority;
        entity.IsActive = request.IsActive;
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        await authorizationDbContext.SaveChangesAsync(cancellationToken);
        return MapPolicy(entity);
    }

    public async Task DeletePolicyAsync(int policyId, CancellationToken cancellationToken)
    {
        var entity = await authorizationDbContext.AuthorizationFieldPolicies
            .FirstOrDefaultAsync(x => x.Id == policyId && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundAppException($"Field policy bulunamadi. id={policyId}");

        authorizationDbContext.AuthorizationFieldPolicies.Remove(entity);
        await authorizationDbContext.SaveChangesAsync(cancellationToken);
    }

    private static AuthorizationFieldDefinitionDto MapDefinition(AuthorizationFieldDefinition entity)
        => new(
            entity.Id,
            entity.EntityName,
            entity.FieldName,
            entity.DisplayName,
            entity.DataType,
            SplitCsv(entity.AllowedSurfaces),
            entity.DefaultVisible,
            entity.DefaultEditable,
            entity.DefaultFilterable,
            entity.DefaultExportable,
            entity.IsActive,
            entity.Description,
            entity.CreatedAt,
            entity.ModifiedAt);

    private static AuthorizationFieldPolicyDto MapPolicy(AuthorizationFieldPolicy entity)
        => new(
            entity.Id,
            entity.Name,
            entity.EntityName,
            entity.FieldName,
            entity.Surface,
            entity.TargetType,
            entity.TargetKey,
            entity.Effect,
            entity.ConditionFieldName,
            entity.ConditionOperator,
            entity.CompareValue,
            entity.MaskingMode,
            entity.Priority,
            entity.IsActive,
            entity.Description,
            entity.CreatedAt,
            entity.ModifiedAt);

    private static void ValidateDefinitionRequest(UpsertAuthorizationFieldDefinitionRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.EntityName))
        {
            errors["entityName"] = ["EntityName zorunludur."];
        }

        if (string.IsNullOrWhiteSpace(request.FieldName))
        {
            errors["fieldName"] = ["FieldName zorunludur."];
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            errors["displayName"] = ["DisplayName zorunludur."];
        }

        if (string.IsNullOrWhiteSpace(request.DataType))
        {
            errors["dataType"] = ["DataType zorunludur."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationAppException("Field definition dogrulamasi basarisiz.", errors);
        }
    }

    private static void ValidatePolicyRequest(UpsertAuthorizationFieldPolicyRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["name"] = ["Name zorunludur."];
        }

        if (string.IsNullOrWhiteSpace(request.EntityName))
        {
            errors["entityName"] = ["EntityName zorunludur."];
        }

        if (string.IsNullOrWhiteSpace(request.FieldName))
        {
            errors["fieldName"] = ["FieldName zorunludur."];
        }

        if (string.IsNullOrWhiteSpace(request.Surface))
        {
            errors["surface"] = ["Surface zorunludur."];
        }

        if (string.IsNullOrWhiteSpace(request.TargetType))
        {
            errors["targetType"] = ["TargetType zorunludur."];
        }

        if (string.IsNullOrWhiteSpace(request.Effect))
        {
            errors["effect"] = ["Effect zorunludur."];
        }

        if (string.IsNullOrWhiteSpace(request.ConditionOperator))
        {
            errors["conditionOperator"] = ["ConditionOperator zorunludur."];
        }

        if (request.Priority < 0)
        {
            errors["priority"] = ["Priority sifirdan kucuk olamaz."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationAppException("Field policy dogrulamasi basarisiz.", errors);
        }
    }

    private static string NormalizeKey(string value)
        => value.Trim().Replace(" ", string.Empty).ToUpperInvariant();

    private static string NormalizeEnumLike(string value)
        => value.Trim().Replace(" ", "_").ToUpperInvariant();

    private static string? NormalizeSurfaces(IReadOnlyList<string>? surfaces)
    {
        if (surfaces is null || surfaces.Count == 0)
        {
            return null;
        }

        return string.Join(
            ',',
            surfaces
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(NormalizeEnumLike)
                .Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<string> SplitCsv(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
