using Authorization.Application.Contracts;

namespace Authorization.Application.Services;

public interface IAuthorizationFieldPolicyAdminService
{
    Task<IReadOnlyList<AuthorizationFieldDefinitionDto>> ListDefinitionsAsync(string? entityName, CancellationToken cancellationToken);
    Task<AuthorizationFieldDefinitionDto> UpsertDefinitionAsync(UpsertAuthorizationFieldDefinitionRequest request, CancellationToken cancellationToken);
    Task DeleteDefinitionAsync(int definitionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AuthorizationFieldPolicyDto>> ListPoliciesAsync(string? entityName, string? fieldName, CancellationToken cancellationToken);
    Task<AuthorizationFieldPolicyDto> UpsertPolicyAsync(UpsertAuthorizationFieldPolicyRequest request, CancellationToken cancellationToken);
    Task DeletePolicyAsync(int policyId, CancellationToken cancellationToken);
}
