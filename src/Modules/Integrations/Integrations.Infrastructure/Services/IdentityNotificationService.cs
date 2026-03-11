using Identity.Application.Services;
using Integrations.Application.Contracts;
using Integrations.Application.Services;

namespace Integrations.Infrastructure.Services;

public sealed class IdentityNotificationService(IExternalOutboxService externalOutboxService) : IIdentityNotificationService
{
    public async Task QueueAdminMailAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        await externalOutboxService.QueueMailAsync(new QueueMailRequest
        {
            To = to,
            Subject = subject,
            Body = body
        }, cancellationToken);
    }
}
