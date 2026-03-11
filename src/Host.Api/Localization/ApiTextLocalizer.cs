using System.Globalization;
using System.Text.Json;

namespace Host.Api.Localization;

public sealed class ApiTextLocalizer(IHostEnvironment hostEnvironment) : IApiTextLocalizer
{
    private readonly Lazy<IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>> _resources =
        new(() => LoadResources(hostEnvironment.ContentRootPath));

    public string Get(string key)
    {
        var language = CultureInfo.CurrentUICulture.Name;
        var resources = _resources.Value;

        // Ilk tercih tam culture adidir.
        // Ornek: tr-TR, en-US, de-DE.
        if (resources.TryGetValue(language, out var cultureTexts) && cultureTexts.TryGetValue(key, out var exactValue))
        {
            return exactValue;
        }

        // Tam eslesme yoksa neutral language fallback uygulanir.
        // Ornek: sadece "en" geldiyse en-US'e duser.
        var neutralLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var fallbackCulture = neutralLanguage switch
        {
            "de" => "de-DE",
            "en" => "en-US",
            _ => "tr-TR"
        };

        if (resources.TryGetValue(fallbackCulture, out var fallbackTexts) && fallbackTexts.TryGetValue(key, out var fallbackValue))
        {
            return fallbackValue;
        }

        if (resources.TryGetValue("en-US", out var englishTexts) && englishTexts.TryGetValue(key, out var englishValue))
        {
            return englishValue;
        }

        // Son emniyet supabi: hicbir kaynakta bulunamazsa key'in kendisini doneriz.
        return key;
    }

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> LoadResources(string contentRootPath)
    {
        var resourcesDirectory = Path.Combine(contentRootPath, "Localization", "Resources");
        if (!Directory.Exists(resourcesDirectory))
        {
            return new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        }

        var resources = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var filePath in Directory.GetFiles(resourcesDirectory, "*.json", SearchOption.TopDirectoryOnly))
        {
            // Her JSON dosyasi bir culture sozlugu gibi davranir.
            // Dosya adi culture adidir: tr-TR.json gibi.
            var cultureName = Path.GetFileNameWithoutExtension(filePath);
            var json = File.ReadAllText(filePath);
            var content = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                          ?? new Dictionary<string, string>(StringComparer.Ordinal);

            resources[cultureName] = new Dictionary<string, string>(content, StringComparer.Ordinal);
        }

        return resources;
    }
}
