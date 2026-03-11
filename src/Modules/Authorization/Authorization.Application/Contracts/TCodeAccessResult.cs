namespace Authorization.Application.Contracts;

public sealed class TCodeAccessResult
{
    public string TransactionCode { get; set; } = string.Empty;
    public string? ModuleCode { get; set; }
    public string? SubModuleCode { get; set; }
    public string? PageCode { get; set; }
    public string? RouteLink { get; set; }
    public bool IsAllowed { get; set; }
    public short? DeniedAtLevel { get; set; }
    public string? DeniedReason { get; set; }
    public bool? AmountVisible { get; set; }
    public Dictionary<string, bool> Actions { get; set; } = new();
    public List<TCodeConditionResult> Conditions { get; set; } = new();
}

public sealed class TCodeConditionResult
{
    public string FieldName { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string ExpectedValue { get; set; } = string.Empty;
    public string? ActualValue { get; set; }
    public bool IsSatisfied { get; set; }
}
