using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

/// <summary>
/// Infrastructure katmanına ait servis kayıtlarını tek noktada toplar.
/// Bu sayede Host tarafında bağlantı ve DbContext detayları dağılmaz.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// BusinessDb ve LogDb context kayıtlarını yapar.
    /// </summary>
    public static IServiceCollection AddInfrastructurePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<LogDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("LogDb");
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", LogDbContext.LogsSchema);
            });
        });

        services.AddDbContext<BusinessDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("BusinessDb");
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", BusinessDbContext.AuthorizationSchema);
            });
        });

        return services;
    }
}
