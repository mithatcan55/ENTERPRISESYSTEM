using Integrations.Application.Contracts;

namespace Integrations.Application.Services;

public interface IExcelReportComposerService
{
    Task<string> ComposeCsvAsync(ExcelOutboxPayload payload, CancellationToken cancellationToken);
}
