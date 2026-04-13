using Application.Security;
using Authorization.Application.Contracts;
using Authorization.Infrastructure.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Auditing;
using Infrastructure.Persistence.Entities.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace UnitTests;

public sealed class AuthorizationFieldPolicyEvaluatorTests
{
    [Fact]
    public async Task EvaluateAsync_Masks_Field_When_Permission_Is_Missing_And_Value_Exceeds_Threshold()
    {
        await using var dbContext = CreateDbContext(nameof(EvaluateAsync_Masks_Field_When_Permission_Is_Missing_And_Value_Exceeds_Threshold));
        SeedPricePolicies(dbContext);

        var httpContextAccessor = CreateHttpContextAccessor(permissions: []);
        var evaluator = new AuthorizationFieldPolicyEvaluator(
            dbContext,
            new FakeCurrentUserContext(userId: 5, roles: ["MATERIAL_USER"]),
            httpContextAccessor);

        var result = await evaluator.EvaluateAsync(
            new EvaluateAuthorizationFieldPolicyRequest(
                "Material",
                "detail",
                new Dictionary<string, string?> { ["PRICE"] = "15000" }),
            CancellationToken.None);

        var price = Assert.Single(result);
        Assert.True(price.Visible);
        Assert.True(price.Masked);
        Assert.Equal("FULL", price.MaskingMode);
    }

    [Fact]
    public async Task EvaluateAsync_Keeps_Field_Visible_When_Required_Permission_Is_Present()
    {
        await using var dbContext = CreateDbContext(nameof(EvaluateAsync_Keeps_Field_Visible_When_Required_Permission_Is_Present));
        SeedPricePolicies(dbContext);

        var httpContextAccessor = CreateHttpContextAccessor(permissions: ["MATERIAL.PRICE.HIGHVALUE.VIEW"]);
        var evaluator = new AuthorizationFieldPolicyEvaluator(
            dbContext,
            new FakeCurrentUserContext(userId: 5, roles: ["MATERIAL_USER"]),
            httpContextAccessor);

        var result = await evaluator.EvaluateAsync(
            new EvaluateAuthorizationFieldPolicyRequest(
                "Material",
                "detail",
                new Dictionary<string, string?> { ["PRICE"] = "15000" }),
            CancellationToken.None);

        var price = Assert.Single(result);
        Assert.True(price.Visible);
        Assert.False(price.Masked);
    }

    private static AuthorizationDbContext CreateDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<AuthorizationDbContext>()
            .UseInMemoryDatabase($"field-policy-{databaseName}")
            .Options;

        return new AuthorizationDbContext(options, new FakeAuditActorAccessor());
    }

    private static void SeedPricePolicies(AuthorizationDbContext dbContext)
    {
        dbContext.AuthorizationFieldDefinitions.Add(new AuthorizationFieldDefinition
        {
            EntityName = "MATERIAL",
            FieldName = "PRICE",
            DisplayName = "Price",
            DataType = "DECIMAL",
            DefaultVisible = true,
            DefaultEditable = true,
            DefaultFilterable = true,
            DefaultExportable = true,
            IsActive = true
        });

        dbContext.AuthorizationFieldPolicies.Add(new AuthorizationFieldPolicy
        {
            Name = "Mask expensive prices by default",
            EntityName = "MATERIAL",
            FieldName = "PRICE",
            Surface = "DETAIL",
            TargetType = "ANY",
            Effect = "MASK",
            ConditionFieldName = "PRICE",
            ConditionOperator = "GT",
            CompareValue = "10000",
            MaskingMode = "FULL",
            Priority = 10,
            IsActive = true
        });

        dbContext.AuthorizationFieldPolicies.Add(new AuthorizationFieldPolicy
        {
            Name = "Finance permission can see expensive prices",
            EntityName = "MATERIAL",
            FieldName = "PRICE",
            Surface = "DETAIL",
            TargetType = "PERMISSION",
            TargetKey = "MATERIAL.PRICE.HIGHVALUE.VIEW",
            Effect = "SHOW",
            ConditionOperator = "ALWAYS",
            Priority = 100,
            IsActive = true
        });

        dbContext.SaveChanges();
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(IReadOnlyList<string> permissions)
    {
        var claims = permissions
            .Select(permission => new System.Security.Claims.Claim(SecurityClaimTypes.Permission, permission))
            .ToList();

        var identity = new System.Security.Claims.ClaimsIdentity(claims, "test");
        var context = new DefaultHttpContext
        {
            User = new System.Security.Claims.ClaimsPrincipal(identity)
        };

        return new HttpContextAccessor
        {
            HttpContext = context
        };
    }

    private sealed class FakeCurrentUserContext(int userId, IReadOnlyList<string> roles) : ICurrentUserContext
    {
        public bool TryGetUserId(out int resolvedUserId)
        {
            resolvedUserId = userId;
            return true;
        }

        public bool TryGetSessionId(out int sessionId)
        {
            sessionId = 1;
            return true;
        }

        public bool TryGetCompanyId(out int companyId)
        {
            companyId = 1;
            return true;
        }

        public bool TryGetUserCode(out string userCode)
        {
            userCode = $"USER_{userId}";
            return true;
        }

        public bool TryGetUsername(out string username)
        {
            username = $"user.{userId}";
            return true;
        }

        public bool TryGetActorIdentity(out string actorIdentity)
        {
            actorIdentity = $"USER_{userId}";
            return true;
        }

        public bool IsInRole(string roleCode)
            => roles.Contains(roleCode, StringComparer.OrdinalIgnoreCase);
    }

    private sealed class FakeAuditActorAccessor : IAuditActorAccessor
    {
        public string GetActorId() => "test";
    }
}
