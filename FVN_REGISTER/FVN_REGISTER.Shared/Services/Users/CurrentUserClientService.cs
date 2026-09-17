using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Shared.Handlers;
using FVN_REGISTER.Shared.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Shared.Services.Users
{
    public class CurrentUserClientService : ICurrentUserClientService
    {
        private readonly ITokenStorage _tokenStorage;
        private readonly IHttpClientWithAuth _authClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILogger<CurrentUserClientService> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;

        private bool Debug => _options.CurrentValue.Enabled;

        public UserIdentityDto? User { get; private set; }
        public bool IsLoggedIn => User != null;

        public CurrentUserClientService(
            ITokenStorage tokenStorage,
            IHttpClientWithAuth authClient,
            AuthenticationStateProvider authStateProvider,
            ILogger<CurrentUserClientService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _tokenStorage = tokenStorage;
            _authClient = authClient;
            _authStateProvider = authStateProvider;
            _logger = logger;
            _options = options;
        }

        public async Task InitializeAsync(string? explicitToken = null)
        {
            try
            {
                string? token = explicitToken;
                if (string.IsNullOrWhiteSpace(token) && _authStateProvider is CustomAuthStateProvider cp)
                    token = cp.CurrentToken;

                if (string.IsNullOrWhiteSpace(token))
                {
                    try { token = await _tokenStorage.GetTokenAsync(); }
                    catch { }
                }

                if (string.IsNullOrWhiteSpace(token))
                {
                    User = null;
                    _logger.LogDebugIf(Debug, "[CurrentUser] No token found");
                    return;
                }

                var cleanToken = token.Trim('"').Trim();
                if (_authStateProvider is CustomAuthStateProvider cp2)
                    cp2.CurrentToken = cleanToken;

                _logger.LogDebugIf(Debug, "[CurrentUser] Fetching profile...");
                var result = await _authClient.GetAsync<UserIdentityDto>("api/auth/profile");

                if (result.IsSuccess && result.Data != null)
                {
                    User = result.Data;
                    User.IsLoggedIn = true;
                    _logger.LogInfoIf(Debug, "[CurrentUser] Loaded user {UserId}", User.UserId);
                }
                else
                {
                    User = null;
                    _logger.LogWarnIf(Debug, "[CurrentUser] Profile load failed | Status={Status}", result.StatusCode);
                    if (result.StatusCode == 401)
                    {
                        await _tokenStorage.RemoveTokenAsync();
                        if (_authStateProvider is CustomAuthStateProvider cp)
                            cp.CurrentToken = null;
                    }
                }
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
                if (_authStateProvider is CustomAuthStateProvider cp)
                    cp.CurrentToken = null;
                _logger.LogInfoIf(Debug, "[CurrentUser] Logout completed");
            }
            catch (Exception ex) { _logger.LogError(ex, "[CurrentUser] Logout failed"); }
        }

        public void ClearUser() => User = null;
    }
}
