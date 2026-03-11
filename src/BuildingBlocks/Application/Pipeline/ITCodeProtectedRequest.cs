namespace Application.Pipeline;

public interface ITCodeProtectedRequest
{
    string TransactionCode { get; }
    string? ActionCode { get; }
    bool DenyOnUnsatisfiedConditions => true;
    IReadOnlyDictionary<string, string?> ContextValues => new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
}
