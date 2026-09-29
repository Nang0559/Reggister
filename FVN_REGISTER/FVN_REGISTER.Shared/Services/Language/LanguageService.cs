using Microsoft.JSInterop;

namespace FVN_REGISTER.Shared.Services.Language;

public sealed class LanguageService : ILanguageService, IAsyncDisposable
{
    private const string StorageKey = "fvn.ui.language";
    private readonly IJSRuntime _js;
    private LanguageCode _current = LanguageCode.Vi;
    private bool _initialized;

    public LanguageService(IJSRuntime js) => _js = js;
    public LanguageCode Current => _current;
    public event EventHandler? LanguageChanged;

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;
        try
        {
            var stored = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            _current = LanguageCodeExtensions.Parse(stored);
            await _js.InvokeVoidAsync("document.documentElement.setAttribute", "lang", _current.ToCulture());
        }
        catch (JSException)
        {
            _current = LanguageCode.Vi;
        }
    }

    public async Task SetLanguageAsync(LanguageCode language)
    {
        if (_current == language && _initialized) return;
        _current = language;
        _initialized = true;
        await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, language == LanguageCode.Ja ? "ja-JP" : "vi-VN");
        await _js.InvokeVoidAsync("document.documentElement.setAttribute", "lang", language.ToCulture());
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public string T(string key) => LanguageCatalog.Get(key, _current);
    public ValueTask DisposeAsync() { LanguageChanged = null; return ValueTask.CompletedTask; }
}
