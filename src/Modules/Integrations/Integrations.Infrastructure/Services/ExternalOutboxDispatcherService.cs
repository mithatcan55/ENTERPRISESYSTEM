using System.Text.Json;
using Infrastructure.Persistence;
using Integrations.Application.Contracts;
using Integrations.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace Integrations.Infrastructure.Services;

public sealed class ExternalOutboxDispatcherService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ExternalOutboxDispatcherService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Dispatcher kucuk araliklarla calisan sonsuz bir is dongusudur.
        // Burada amac uygulama ayakta kaldigi surece uygun outbox mesajlarini partiler halinde islemektir.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox dispatcher dongusunde hata olustu.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task DispatchBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var integrationsDbContext = scope.ServiceProvider.GetRequiredService<IntegrationsDbContext>();
        var emailDeliveryService = scope.ServiceProvider.GetRequiredService<IEmailDeliveryService>();
        var excelReportComposerService = scope.ServiceProvider.GetRequiredService<IExcelReportComposerService>();

        var now = DateTime.UtcNow;

        var candidates = await integrationsDbContext.ExternalOutboxMessages
            .Where(x => (x.Status == "Pending" || x.Status == "Failed")
                        && x.NextAttemptAt <= now
                        && x.AttemptCount < x.MaxAttempts)
            .OrderBy(x => x.NextAttemptAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        // Her dongude sinirli sayida mesaj alip sistemin tek bir buyuk batch'e kilitlenmesini engelliyoruz.
        foreach (var message in candidates)
        {
            message.Status = "Processing";
            message.AttemptCount += 1;
            await integrationsDbContext.SaveChangesAsync(cancellationToken);

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
                                $"Excel raporu hazir: {payload.ReportName}",
                                "Rapor olusturuldu ve ektedir.",
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
                // Retry stratejisi exponential backoff mantigi ile kuruldu.
                // Tekrarlanabilir gecici hatalarda sistemi sakin tutmak icin bekleme suresi giderek artar.
                var hasRemainingAttempts = message.AttemptCount < message.MaxAttempts;
                message.Status = hasRemainingAttempts ? "Failed" : "DeadLetter";
                message.LastError = ex.Message.Length > 3500 ? ex.Message[..3500] : ex.Message;
                var delaySeconds = Math.Min(300, (int)Math.Pow(2, message.AttemptCount));
                message.NextAttemptAt = DateTime.UtcNow.AddSeconds(delaySeconds);
                logger.LogWarning(ex, "Outbox mesaji islenemedi. Id={MessageId}, EventType={EventType}", message.Id, message.EventType);
            }

            await integrationsDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
