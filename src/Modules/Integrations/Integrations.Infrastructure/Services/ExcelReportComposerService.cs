using System.Text;
using Integrations.Application.Contracts;
using Integrations.Application.Services;

namespace Integrations.Infrastructure.Services;

public sealed class ExcelReportComposerService : IExcelReportComposerService
{
    public async Task<string> ComposeCsvAsync(ExcelOutboxPayload payload, CancellationToken cancellationToken)
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "outbox-exports");
        Directory.CreateDirectory(directory);

        var fileName = $"{Sanitize(payload.ReportName)}-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        var filePath = Path.Combine(directory, fileName);

        var sb = new StringBuilder();

        if (payload.Headers.Count > 0)
        {
            sb.AppendLine(string.Join(',', payload.Headers.Select(EscapeCsv)));
        }

        foreach (var row in payload.Rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            sb.AppendLine(string.Join(',', row.Select(EscapeCsv)));
        }

        await File.WriteAllTextAsync(filePath, sb.ToString(), cancellationToken);
        return filePath;
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "\"\"";
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static string Sanitize(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(value.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
    }
}
