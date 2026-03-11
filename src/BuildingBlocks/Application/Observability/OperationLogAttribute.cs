namespace Application.Observability;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class OperationLogAttribute(string? operationName = null, string? category = null) : Attribute
{
    public string? OperationName { get; } = operationName;
    public string? Category { get; } = category;
}
