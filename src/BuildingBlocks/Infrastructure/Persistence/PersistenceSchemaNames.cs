namespace Infrastructure.Persistence;

/// <summary>
/// Runtime context'lerin ayni fiziksel veritabani uzerinde ortak schema adlarini
/// merkezi bir yerden kullanmasini saglar.
/// </summary>
public static class PersistenceSchemaNames
{
    public const string Business = "authorizeSchema";
    public const string Logs = "logs";
}
