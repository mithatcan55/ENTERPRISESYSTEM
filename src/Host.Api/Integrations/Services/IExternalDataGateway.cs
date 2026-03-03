using Host.Api.Integrations.Contracts;

namespace Host.Api.Integrations.Services;

public interface IExternalDataGateway
{
    Task<ReferenceCompanyDto> GetReferenceCompanyAsync(int externalId, CancellationToken cancellationToken);
}
