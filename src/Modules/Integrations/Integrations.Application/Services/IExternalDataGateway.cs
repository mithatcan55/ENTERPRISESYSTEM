using Integrations.Application.Contracts;

namespace Integrations.Application.Services;

public interface IExternalDataGateway
{
    Task<ReferenceCompanyDto> GetReferenceCompanyAsync(int externalId, CancellationToken cancellationToken);
}
