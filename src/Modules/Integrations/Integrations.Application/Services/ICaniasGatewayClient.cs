using Integrations.Application.Contracts;

namespace Integrations.Application.Services;

public interface ICaniasGatewayClient
{
    /// <summary>
    /// CaniasGateway'deki bir endpoint'i calistirip JSON sonuc doner.
    /// </summary>
    Task<ErpQueryResult> RunAsync(string endpoint, Dictionary<string, string>? parameters, CancellationToken ct = default);

    /// <summary>
    /// Endpoint'in parametre bilgilerini getirir.
    /// </summary>
    Task<ErpServiceInfo> GetParamsAsync(string endpoint, CancellationToken ct = default);

    /// <summary>
    /// CaniasGateway'deki tum servisleri listeler.
    /// </summary>
    Task<IReadOnlyList<ErpServiceListItem>> ListServicesAsync(CancellationToken ct = default);
}
