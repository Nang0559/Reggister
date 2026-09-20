using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Shared.Utils;

namespace FVN_REGISTER.Services;

#pragma warning disable CA1416 // MAUI SecureStorage is supplied by the target platform; this service is registered only by the MAUI client.
public sealed class SecureTokenService : ITokenStorage
{
    private const string RefreshTokenKey = "fvn.refresh_token";

    public Task SetTokenAsync(string token) =>
        SecureStorage.Default.SetAsync(AuthConstants.TokenKey, token);

    public Task<string?> GetTokenAsync() =>
        SecureStorage.Default.GetAsync(AuthConstants.TokenKey);

    public Task SetRefreshTokenAsync(string refreshToken) =>
        SecureStorage.Default.SetAsync(RefreshTokenKey, refreshToken);

    public Task<string?> GetRefreshTokenAsync() =>
        SecureStorage.Default.GetAsync(RefreshTokenKey);

    public Task RemoveTokenAsync()
    {
        SecureStorage.Default.Remove(AuthConstants.TokenKey);
        return Task.CompletedTask;
    }

    public Task RemoveRefreshTokenAsync()
    {
        SecureStorage.Default.Remove(RefreshTokenKey);
        return Task.CompletedTask;
    }

    public async Task ClearAsync()
    {
        RemoveToken();
        RemoveRefreshToken();
        await Task.CompletedTask;
    }

    private static void RemoveToken() =>
        SecureStorage.Default.Remove(AuthConstants.TokenKey);

    private static void RemoveRefreshToken() =>
        SecureStorage.Default.Remove(RefreshTokenKey);

    public void MarkJsReady()
    {
        // No browser JS lifecycle is required for MAUI SecureStorage.
    }
}
#pragma warning restore CA1416
