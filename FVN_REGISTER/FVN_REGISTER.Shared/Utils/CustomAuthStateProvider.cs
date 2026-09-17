using FVN_REGISTER.Shared.Utils.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FVN_REGISTER.Shared.Utils
{
    public sealed class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILogger<CustomAuthStateProvider> _logger;
        private readonly bool _debug;
        private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

        public string? CurrentToken { get; set; }

        public CustomAuthStateProvider(
            ILogger<CustomAuthStateProvider> logger,
            IConfiguration config)
        {
            _logger = logger;
            _debug = config.GetValue<bool>("AuthDebug:Enabled");
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CurrentToken))
                    return Task.FromResult(new AuthenticationState(_anonymous));

                var claims = JwtParser.ParseClaimsFromJwt(CurrentToken);
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
                return Task.FromResult(new AuthenticationState(user));
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[AuthState] Invalid token");
                return Task.FromResult(new AuthenticationState(_anonymous));
            }
        }

        public void NotifyUserLogin(string token)
        {
            try
            {
                CurrentToken = token.Trim('"').Trim();
                var claims = JwtParser.ParseClaimsFromJwt(CurrentToken);
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));

                if (_debug)
                    _logger.LogInformation("[AuthState] User login notified");

                NotifyAuthenticationStateChanged(
                    Task.FromResult(new AuthenticationState(user)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthState] NotifyUserLogin failed");
                NotifyUserLogout();
            }
        }

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
