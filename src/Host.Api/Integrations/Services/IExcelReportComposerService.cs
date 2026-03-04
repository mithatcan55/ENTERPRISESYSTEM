using Host.Api.Integrations.Contracts;

namespace Host.Api.Integrations.Services;

public interface IExcelReportComposerService
{
    Task<string> ComposeCsvAsync(ExcelOutboxPayload payload, CancellationToken cancellationToken);
}
