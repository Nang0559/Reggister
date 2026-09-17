using FVN_REGISTER.Contract.Dtos.Usermanagers;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Shared.Services.Users
{
    public class UserManagementClientService : IUserManagementClientService
    {
        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<UserManagementClientService> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;
        private bool Debug => _options.CurrentValue.Enabled;
        private const string Base = "api/user-management";

        public UserManagementClientService(
            IHttpClientWithAuth http,
            ILogger<UserManagementClientService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _http = http;
            _logger = logger;
            _options = options;
        }

        public Task<ApiResponse<List<UserAccountDto>>> GetAllAsync(
            CancellationToken ct = default)
            => _http.GetAsync<List<UserAccountDto>>(Base, ct);

        public Task<ApiResponse<UserAccountDto>> GetByIdAsync(
            int id, CancellationToken ct = default)
            => _http.GetAsync<UserAccountDto>($"{Base}/{id}", ct);

        public Task<ApiResponse<object>> CreateAsync(
            CreateUserRequest request, CancellationToken ct = default)
            => _http.PostAsync<object>(Base, request, ct);

        public Task<ApiResponse<object>> UpdateAsync(
            UpdateUserRequest request, CancellationToken ct = default)
            => _http.PutAsync<object>(Base, request, ct);

        public Task<ApiResponse<object>> DeleteAsync(
            int id, CancellationToken ct = default)
            => _http.DeleteAsync<object>($"{Base}/{id}", ct);

        public Task<ApiResponse<object>> ToggleLockAsync(
            int id, CancellationToken ct = default)
            => _http.PutAsync<object>($"{Base}/{id}/toggle-lock", new { }, ct);

        public Task<ApiResponse<object>> ResetPasswordAsync(
            int id, string newPassword, CancellationToken ct = default)
            => _http.PutAsync<object>(
                $"{Base}/{id}/reset-password",
                new { NewPassword = newPassword },
                ct);
    }
}
