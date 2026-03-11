namespace Host.Api.Observability;

public interface ISensitiveDataRedactor
{
    string RedactHeaders(IReadOnlyDictionary<string, string> headers);
    string RedactBody(string? body, string? contentType, PathString path);
    bool ShouldSkipBodyLogging(PathString path);
}
