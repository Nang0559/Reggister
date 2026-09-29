namespace FVN_REGISTER.Shared.Services.Language;

public interface ILanguageService
{
    LanguageCode Current { get; }
    event EventHandler? LanguageChanged;
    Task InitializeAsync();
    Task SetLanguageAsync(LanguageCode language);
    string T(string key);
}
