using System.Text.Json;
using Host.Api.Integrations.Contracts;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Host.Api.Integrations.Services;

public sealed class ExternalOutboxDispatcherService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ExternalOutboxDispatcherService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox dispatcher döngüsünde hata oluştu.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task DispatchBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var businessDbContext = scope.ServiceProvider.GetRequiredService<BusinessDbContext>();
        var emailDeliveryService = scope.ServiceProvider.GetRequiredService<IEmailDeliveryService>();
        var excelReportComposerService = scope.ServiceProvider.GetRequiredService<IExcelReportComposerService>();

        var now = DateTime.UtcNow;

        var candidates = await businessDbContext.ExternalOutboxMessages
            .Where(x => (x.Status == "Pending" || x.Status == "Failed")
                        && x.NextAttemptAt <= now
                        && x.AttemptCount < x.MaxAttempts)
            .OrderBy(x => x.NextAttemptAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in candidates)
        {
            message.Status = "Processing";
            message.AttemptCount += 1;
            await businessDbContext.SaveChangesAsync(cancellationToken);

            try
            {
                switch (message.EventType)
                {
                    case OutboxEventTypes.MailNotification:
                        {
                            var payload = JsonSerializer.Deserialize<MailOutboxPayload>(message.PayloadJson)
                                          ?? throw new InvalidOperationException("Mail payload parse edilemedi.");

                            await emailDeliveryService.SendAsync(payload.To, payload.Subject, payload.Body, null, cancellationToken);
                            break;
                        }
                    case OutboxEventTypes.ExcelReport:
                        {
                            var payload = JsonSerializer.Deserialize<ExcelOutboxPayload>(message.PayloadJson)
                                          ?? throw new InvalidOperationException("Excel payload parse edilemedi.");

                            var generatedPath = await excelReportComposerService.ComposeCsvAsync(payload, cancellationToken);
                            if (!string.IsNullOrWhiteSpace(payload.NotifyEmail))
                            {
                                await emailDeliveryService.SendAsync(
                                    payload.NotifyEmail,
                                    $"Excel raporu hazır: {payload.ReportName}",
                                    "Rapor oluşturuldu ve ektedir.",
                                    generatedPath,
                                    cancellationToken);
                            }
                            break;
                        }
                    default:
                        throw new InvalidOperationException($"Bilinmeyen outbox event type: {message.EventType}");
                }

                message.Status = "Succeeded";
                message.ProcessedAt = DateTime.UtcNow;
                message.LastError = null;
            }
            catch (Exception ex)
            {
                var hasRemainingAttempts = message.AttemptCount < message.MaxAttempts;
                message.Status = hasRemainingAttempts ? "Failed" : "DeadLetter";
                message.LastError = ex.Message.Length > 3500 ? ex.Message[..3500] : ex.Message;
                var delaySeconds = Math.Min(300, (int)Math.Pow(2, message.AttemptCount));
                message.NextAttemptAt = DateTime.UtcNow.AddSeconds(delaySeconds);
                logger.LogWarning(ex, "Outbox mesajı işlenemedi. Id={MessageId}, EventType={EventType}", message.Id, message.EventType);
            }

            await businessDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
