using Microsoft.Extensions.DependencyInjection;

namespace Identity.Presentation.DependencyInjection;

/// <summary>
/// Identity modülüne ait presentation katmanı servis kayıtlarını toplar.
/// Şu an modül iskelet olduğu için sadece modül giriş noktası görevi görür.
/// </summary>
public static class IdentityPresentationServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityPresentationModule(this IServiceCollection services)
    {
        return services;
    }
}
