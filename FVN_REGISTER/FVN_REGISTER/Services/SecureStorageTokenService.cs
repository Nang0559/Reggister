
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Shared.Utils;


namespace FVN_REGISTER.Services;

public sealed class SecureTokenService : ITokenStorage
{
    public Task SetTokenAsync(string token) =>
        SecureStorage.Default.SetAsync(AuthConstants.TokenKey, token);

    public Task<string?> GetTokenAsync() =>
        SecureStorage.Default.GetAsync(AuthConstants.TokenKey);

    public Task RemoveTokenAsync()
    {
        SecureStorage.Default.Remove(AuthConstants.TokenKey);
        return Task.CompletedTask;
    }

    public void MarkJsReady()
    {
        // No browser JS lifecycle is required for MAUI SecureStorage.
    }
}
