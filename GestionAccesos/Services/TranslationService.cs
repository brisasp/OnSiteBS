using System.Text.Json;

namespace GestionAccesos.Services;

public class TranslationService
{
    private readonly IWebHostEnvironment _env;
    private Dictionary<string, string> _translations = new();
    public string CurrentCulture { get; private set; } = "es";
    public event Action? OnLanguageChanged;

    public TranslationService(IWebHostEnvironment env)
    {
        _env = env;
        Load("es");
    }

    public void Load(string culture)
    {
        var path = Path.Combine(_env.WebRootPath, "i18n", $"{culture}.json");

        if (!File.Exists(path))
        {
            _translations = new();
            CurrentCulture = "es";
            return;
        }

        var json = File.ReadAllText(path);
        _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
        CurrentCulture = culture;
        OnLanguageChanged?.Invoke();
    }

    public string this[string key] => _translations.TryGetValue(key, out var value) ? value : $"[{key}]";

    public string this[string key, params object[] args]
    => _translations.TryGetValue(key, out var format)
        ? string.Format(format, args)
        : $"[{key}]";
}