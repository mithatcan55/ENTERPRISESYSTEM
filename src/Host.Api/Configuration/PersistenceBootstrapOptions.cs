namespace Host.Api.Configuration;

/// <summary>
/// Baslangicta calisacak veritabani hazirlama davranislarini konfigurasyonla acip kapatir.
/// Legacy BusinessDbContext migration koprusu tamamen silinmeden once kontrollu gecis icin kullanilir.
/// </summary>
public sealed class PersistenceBootstrapOptions
{
    public const string SectionName = "PersistenceBootstrap";

    public bool EnableLegacyBusinessContextMigration { get; set; } = true;
}
