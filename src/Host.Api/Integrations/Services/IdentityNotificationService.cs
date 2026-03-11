using Host.Api.Integrations.Contracts;
using Identity.Application.Services;

namespace Host.Api.Integrations.Services;

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
