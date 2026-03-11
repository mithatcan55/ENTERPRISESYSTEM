namespace Infrastructure.Observability;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SkipOperationLogAttribute : Attribute
{
}
