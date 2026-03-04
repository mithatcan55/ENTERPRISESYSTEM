using Host.Api.Operations.Contracts;

namespace Host.Api.Operations.Services;

public interface IAuditDashboardService
{
    Task<AuditDashboardSummaryDto> GetSummaryAsync(int windowHours, CancellationToken cancellationToken);
}
