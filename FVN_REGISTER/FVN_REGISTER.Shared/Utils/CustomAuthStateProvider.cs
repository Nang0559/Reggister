using FVN_REGISTER.Shared.Utils.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;


namespace FVN_REGISTER.Shared.Utils
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ITokenStorage _tokenStorage;
        private readonly ILogger<CustomAuthStateProvider> _logger;
        private readonly bool _debug;

        private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

        public string? CurrentToken { get; set; }

        public CustomAuthStateProvider(
            ITokenStorage tokenStorage,
            ILogger<CustomAuthStateProvider> logger,
            IConfiguration config)
        {
            _tokenStorage = tokenStorage;
            _logger = logger;

            _debug = config.GetValue<bool>("AuthDebug:Enabled");

            _logger.LogInformation("[AuthState] Initialized");
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // 🔥 CHỈ dùng RAM
                var token = CurrentToken;

                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogInformation("[AuthState] Anonymous user");
                    return Task.FromResult(new AuthenticationState(_anonymous));
                }

                var claims = JwtParser.ParseClaimsFromJwt(token);
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));

                _logger.LogInformation("[AuthState] Authenticated");
                return Task.FromResult(new AuthenticationState(user));
            }
            catch
            {
                return Task.FromResult(new AuthenticationState(_anonymous));
            }
        }

        // ================= LOGIN =================
        public void NotifyUserLogin(string token)
        {
            try
            {
                CurrentToken = token?.Trim('"').Trim();

                var claims = JwtParser.ParseClaimsFromJwt(CurrentToken!);
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));

                if (_debug)
                    _logger.LogInformation("[AuthState] User login notified");

                NotifyAuthenticationStateChanged(
                    Task.FromResult(new AuthenticationState(user)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthState] NotifyUserLogin failed");
            }
        }

        // ================= LOGOUT =================
        public void NotifyUserLogout()
        {
            CurrentToken = null;

            if (_debug)
                _logger.LogInformation("[AuthState] User logout");

            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(_anonymous)));
        }
    }
}
