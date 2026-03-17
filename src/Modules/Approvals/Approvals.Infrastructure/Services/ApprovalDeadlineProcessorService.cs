using Application.Observability;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Approvals.Infrastructure.Services;

public sealed class ApprovalDeadlineProcessorService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ApprovalDeadlineProcessorService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredStepsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Approval deadline processor dongusunde hata olustu.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task ProcessExpiredStepsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<ApprovalDeadlineProcessor>();
        await processor.ProcessBatchAsync(cancellationToken);
    }
}
