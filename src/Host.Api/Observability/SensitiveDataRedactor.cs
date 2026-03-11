using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Host.Api.Observability;

public sealed class SensitiveDataRedactor(IOptions<SensitiveDataLoggingOptions> options) : ISensitiveDataRedactor
{
    private readonly SensitiveDataLoggingOptions _options = options.Value;

    public string RedactHeaders(IReadOnlyDictionary<string, string> headers)
    {
        var sanitized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var header in headers)
        {
            sanitized[header.Key] = IsSensitiveHeader(header.Key)
                ? _options.MaskValue
                : header.Value;
        }

        return JsonSerializer.Serialize(sanitized);
    }

    public string RedactBody(string? body, string? contentType, PathString path)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return string.Empty;
        }

        if (ShouldSkipBodyLogging(path))
        {
            return _options.MaskValue;
        }

        if (!IsJson(contentType))
        {
            return body;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var sanitized = SanitizeElement(document.RootElement);
            return JsonSerializer.Serialize(sanitized);
        }
        catch (JsonException)
        {
            return body;
        }
    }

    public bool ShouldSkipBodyLogging(PathString path)
    {
        var requestPath = path.Value ?? string.Empty;
        return _options.ExcludedPaths.Any(excluded =>
            requestPath.StartsWith(excluded, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsSensitiveHeader(string headerName)
    {
        return _options.RedactedHeaders.Any(header =>
            string.Equals(header, headerName, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsSensitiveField(string fieldName)
    {
        return _options.RedactedJsonFields.Any(field =>
            string.Equals(field, fieldName, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsJson(string? contentType)
    {
        return !string.IsNullOrWhiteSpace(contentType)
               && contentType.Contains("json", StringComparison.OrdinalIgnoreCase);
    }

    private object? SanitizeElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => SanitizeObject(element),
            JsonValueKind.Array => element.EnumerateArray().Select(SanitizeElement).ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var longValue)
                ? longValue
                : element.TryGetDecimal(out var decimalValue)
                    ? decimalValue
                    : element.GetRawText(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };
    }

    private Dictionary<string, object?> SanitizeObject(JsonElement element)
    {
        var sanitized = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in element.EnumerateObject())
        {
            sanitized[property.Name] = IsSensitiveField(property.Name)
                ? _options.MaskValue
                : SanitizeElement(property.Value);
        }

        return sanitized;
    }
}
