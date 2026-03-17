using System.Text.Json;
using Application.Exceptions;
using global::Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Approvals.Infrastructure.Workflows;

/// <summary>
/// Ilk fazda approver tipi olarak specific_user ve role desteklenir.
/// Aktif delegation varsa atama dogrudan vekil kullaniciya cevrilir.
/// </summary>
public sealed class ApproverResolutionService(
    IdentityDbContext identityDbContext,
    ApprovalsDbContext approvalsDbContext)
{
    public async Task<IReadOnlyList<int?>> ResolveAssignedUserIdsAsync(string workflowCode, string approverType, string approverValue, CancellationToken cancellationToken)
    {
        var resolvedUserIds = approverType.Trim().ToLowerInvariant() switch
        {
            "specific_user" => await ResolveSpecificUserAsync(approverValue, cancellationToken),
            "role" => await ResolveRoleUsersAsync(approverValue, cancellationToken),
            _ => throw new ValidationAppException("Desteklenmeyen approver type.", new Dictionary<string, string[]>
            {
                ["approverType"] = [$"Approver type desteklenmiyor: {approverType}"]
            })
        };

        var delegatedUserIds = new List<int?>();
        foreach (var userId in resolvedUserIds)
        {
            delegatedUserIds.Add(await ResolveDelegationAsync(workflowCode, userId, cancellationToken));
        }

        return delegatedUserIds.Distinct().ToList();
    }

    private async Task<IReadOnlyList<int>> ResolveSpecificUserAsync(string approverValue, CancellationToken cancellationToken)
    {
        if (int.TryParse(approverValue, out var userId))
        {
            var exists = await identityDbContext.Users.AnyAsync(x => !x.IsDeleted && x.IsActive && x.Id == userId, cancellationToken);
            if (!exists)
            {
                throw new NotFoundAppException($"Specific approver kullanicisi bulunamadi. UserId={userId}");
            }

            return [userId];
        }

        var resolvedUser = await identityDbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.IsActive && x.UserCode == approverValue, cancellationToken)
            ?? throw new NotFoundAppException($"Specific approver kullanicisi bulunamadi. UserCode={approverValue}");

        return [resolvedUser.Id];
    }

    private async Task<IReadOnlyList<int>> ResolveRoleUsersAsync(string roleCode, CancellationToken cancellationToken)
    {
        var query =
            from userRole in identityDbContext.UserRoles.AsNoTracking()
            join role in identityDbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
            join user in identityDbContext.Users.AsNoTracking() on userRole.UserId equals user.Id
            where !userRole.IsDeleted && !role.IsDeleted && !user.IsDeleted && user.IsActive && role.Code == roleCode
            orderby user.Id
            select user.Id;

        var userIds = await query.Distinct().ToListAsync(cancellationToken);

        if (userIds.Count == 0)
        {
            throw new NotFoundAppException($"Role approver icin aktif kullanici bulunamadi. Role={roleCode}");
        }

        return userIds;
    }

    private async Task<int?> ResolveDelegationAsync(string workflowCode, int userId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var assignments = await approvalsDbContext.DelegationAssignments
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive && x.DelegatorUserId == userId && x.StartsAt <= now && x.EndsAt >= now)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        foreach (var assignment in assignments)
        {
            if (MatchesDelegationScope(workflowCode, assignment.ScopeType, assignment.IncludedScopesJson, assignment.ExcludedScopesJson))
            {
                return assignment.DelegateUserId;
            }
        }

        return userId;
    }

    private static bool MatchesDelegationScope(string workflowCode, string scopeType, string includedScopesJson, string excludedScopesJson)
    {
        var included = ParseStringArray(includedScopesJson);
        var excluded = ParseStringArray(excludedScopesJson);

        if (excluded.Any(x => string.Equals(x, workflowCode, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return scopeType.Trim().ToLowerInvariant() switch
        {
            "all" => true,
            "workflow" => included.Count == 0 || included.Any(x => string.Equals(x, workflowCode, StringComparison.OrdinalIgnoreCase)),
            _ => false
        };
    }

    private static IReadOnlyList<string> ParseStringArray(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            var result = JsonSerializer.Deserialize<List<string>>(json);
            return result ?? [];
        }
        catch
        {
            return [];
        }
    }
}
