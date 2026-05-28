using Blazored.LocalStorage;
using GestionAccesos.Services;
using static GestionAccesos.Enums;

namespace GestionAccesos.Helpers;

public static class IdiomaHelper
{
    public static async Task CargarIdiomaAsync(ILocalStorageService localStorage, TranslationService translationService)
    {
        var lang = await localStorage.GetItemAsync<string>("selectedLanguage");
        translationService.Load(lang ?? "es");
    }

    public static async Task CambiarIdiomaAsync(Enums.Idiomas idioma, ILocalStorageService storage, TranslationService translator)
    {
        string culture = idioma switch
        {
            Idiomas.Español => "es",
            Idiomas.English => "en",
            Idiomas.Français => "fr",
            Idiomas.العربية => "ar",
            _ => "es"
        };

        translator.Load(culture);
        await storage.SetItemAsync("selectedLanguage", culture);
    }
}