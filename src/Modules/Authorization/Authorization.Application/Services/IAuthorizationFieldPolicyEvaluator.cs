using Authorization.Application.Contracts;

namespace Authorization.Application.Services;

public interface IAuthorizationFieldPolicyEvaluator
{
    Task<IReadOnlyList<AuthorizationFieldPolicyDecisionDto>> EvaluateAsync(
        EvaluateAuthorizationFieldPolicyRequest request,
        CancellationToken cancellationToken);
}
