using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Shared.Utils;
using Microsoft.JSInterop;

namespace FVN_REGISTER.Web.Services;

public sealed class LocalStorageTokenService : ITokenStorage
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<LocalStorageTokenService> _logger;
    private readonly bool _debug;

    // Scoped per Blazor Server circuit. Never share JWTs between users.
    private string? _cachedToken;
    private string? _cachedRefreshToken;

    public LocalStorageTokenService(
        IJSRuntime jsRuntime,
        ILogger<LocalStorageTokenService> logger,
        IConfiguration configuration)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
        _debug = configuration.GetValue<bool>("AuthDebug:Enabled");
    }

    public void MarkJsReady()
    {
        if (_debug)
            _logger.LogDebug("[TokenStorage] JS runtime marked ready.");
    }

    public async Task<string?> GetTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_cachedToken))
            return _cachedToken;

        try
        {
            var token = await _jsRuntime.InvokeAsync<string?>(
                "localStorage.getItem",
                AuthConstants.TokenKey);

            _cachedToken = token?.Trim('"').Trim();

            if (_debug)
                _logger.LogDebug("[TokenStorage] Token loaded from browser storage.");

            return _cachedToken;
        }
        catch (InvalidOperationException ex)
        {
            // JS can be unavailable during an early/prerender lifecycle.
            if (_debug)
                _logger.LogDebug(ex, "[TokenStorage] JS runtime is not ready.");
            return _cachedToken;
        }
        catch (JSException ex)
        {
            if (_debug)
                _logger.LogDebug(ex, "[TokenStorage] Browser storage access failed.");
            return _cachedToken;
        }
    }

    public async Task SetRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return;
        _cachedRefreshToken = refreshToken.Trim('"').Trim();
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", AuthConstants.RefreshTokenKey, _cachedRefreshToken);
        }
        catch (Exception ex) when (ex is InvalidOperationException or JSException)
        {
            if (_debug) _logger.LogDebug(ex, "[TokenStorage] Refresh token storage unavailable.");
        }
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_cachedRefreshToken)) return _cachedRefreshToken;
        try
        {
            _cachedRefreshToken = (await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", RefreshTokenKey))?.Trim('"').Trim();
        }
        catch (Exception ex) when (ex is InvalidOperationException or JSException)
        {
            if (_debug) _logger.LogDebug(ex, "[TokenStorage] Refresh token unavailable.");
        }
        return _cachedRefreshToken;
    }

    public async Task RemoveRefreshTokenAsync()
    {
        _cachedRefreshToken = null;
        try { await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", AuthConstants.RefreshTokenKey); }
        catch (Exception ex) when (ex is InvalidOperationException or JSException) { if (_debug) _logger.LogDebug(ex, "[TokenStorage] Refresh token removal unavailable."); }
    }

    public async Task ClearAsync()
    {
        await RemoveTokenAsync();
        await RemoveRefreshTokenAsync();
    }

    public async Task SetTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return;

        _cachedToken = token.Trim('"').Trim();

        try
        {
            await _jsRuntime.InvokeVoidAsync(
                "localStorage.setItem",
                AuthConstants.TokenKey,
                _cachedToken);

            if (_debug)
                _logger.LogDebug("[TokenStorage] Token saved.");
        }
        catch (InvalidOperationException ex)
        {
            if (_debug)
                _logger.LogDebug(ex, "[TokenStorage] JS runtime is not ready; token remains in circuit cache.");
        }
        catch (JSException ex)
        {
            if (_debug)
                _logger.LogDebug(ex, "[TokenStorage] Browser storage write failed; token remains in circuit cache.");
        }
    }

    public async Task RemoveTokenAsync()
    {
        _cachedToken = null;

        try
        {
            await _jsRuntime.InvokeVoidAsync(
                "localStorage.removeItem",
                AuthConstants.TokenKey);
        }
        catch (InvalidOperationException ex)
        {
            if (_debug)
                _logger.LogDebug(ex, "[TokenStorage] JS runtime is not ready during token removal.");
        }
        catch (JSException ex)
        {
            if (_debug)
                _logger.LogDebug(ex, "[TokenStorage] Browser storage removal failed.");
        }
    }
}
