using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Shared.Handlers;
using FVN_REGISTER.Shared.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Shared.Services.Users
{
    public sealed class CurrentUserClientService : ICurrentUserClientService
    {
        private readonly ITokenStorage _tokenStorage;
        private readonly IHttpClientWithAuth _authClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILogger<CurrentUserClientService> _logger;

        public UserIdentityDto? User { get; private set; }
        public bool IsLoggedIn => User != null;

        public CurrentUserClientService(
            ITokenStorage tokenStorage,
            IHttpClientWithAuth authClient,
            AuthenticationStateProvider authStateProvider,
            ILogger<CurrentUserClientService> logger)
        {
            _tokenStorage = tokenStorage;
            _authClient = authClient;
            _authStateProvider = authStateProvider;
            _logger = logger;
        }

        public async Task InitializeAsync(string? explicitToken = null)
        {
            try
            {
                string? token = explicitToken;
                if (string.IsNullOrWhiteSpace(token) && _authStateProvider is CustomAuthStateProvider provider)
                    token = provider.CurrentToken;

                if (string.IsNullOrWhiteSpace(token))
                {
                    try { token = await _tokenStorage.GetTokenAsync(); }
                    catch (Exception ex) { _logger.LogDebug(ex, "[CurrentUser] Token storage read failed"); }
                }

                if (string.IsNullOrWhiteSpace(token))
                {
                    User = null;
                    return;
                }

                var cleanToken = token.Trim('"').Trim();
                if (_authStateProvider is CustomAuthStateProvider currentProvider)
                    currentProvider.CurrentToken = cleanToken;

                var result = await _authClient.GetAsync<UserIdentityDto>("api/auth/profile");

                if (result.IsSuccess && result.Data != null)
                {
                    User = result.Data;
                    User.IsLoggedIn = true;
                    _logger.LogDebug("[CurrentUser] Loaded user {UserId}", User.UserId);
                }
                else
                {
                    User = null;
                    _logger.LogWarning(
                        "[CurrentUser] Profile load failed | Status={Status}",
                        result.StatusCode);

                    if (result.StatusCode == 401)
                    {
                        await _tokenStorage.RemoveTokenAsync();
                        if (_authStateProvider is CustomAuthStateProvider expiredProvider)
                            expiredProvider.CurrentToken = null;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CurrentUser] Initialize failed");
                User = null;
            }
        }

        public async Task LogoutInternal()
        {
            try
            {
                User = null;
                await _tokenStorage.RemoveTokenAsync();
                if (_authStateProvider is CustomAuthStateProvider provider)
                    provider.CurrentToken = null;
                _logger.LogDebug("[CurrentUser] Logout completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CurrentUser] Logout failed");
            }
        }

        public void ClearUser() => User = null;
    }
}
