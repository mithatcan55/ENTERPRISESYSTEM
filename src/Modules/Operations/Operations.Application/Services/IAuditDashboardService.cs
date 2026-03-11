using Operations.Application.Contracts;

namespace Operations.Application.Services;

public interface IAuditDashboardService
{
    Task<AuditDashboardSummaryDto> GetSummaryAsync(int windowHours, CancellationToken cancellationToken);
}
