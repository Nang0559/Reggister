using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.ViewModels.Departments;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Shared.Services.Departments
{
    public class DepartmentManagementClientService : IDepartmentManagementClientService
    {
        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<DepartmentManagementClientService> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;

        private bool Debug => _options.CurrentValue.Enabled;

        private const string BaseUrl = "api/admin/departments";

        public DepartmentManagementClientService(
            IHttpClientWithAuth http,
            ILogger<DepartmentManagementClientService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _http = http;
            _logger = logger;
            _options = options;
        }

        public Task<ApiResponse<DepartmentTreeViewModel>> GetTreeAsync(CancellationToken ct = default)
        {
            _logger.LogDebugIf(Debug, "[DEPT-CLIENT] GetTree");
            return _http.GetAsync<DepartmentTreeViewModel>($"{BaseUrl}/tree", ct);
        }

        public Task<ApiResponse<List<DepartmentItemViewModel>>> GetListAsync(bool? isActive, CancellationToken ct = default)
        {
            var url = isActive.HasValue
                ? $"{BaseUrl}?isActive={isActive.Value}"
                : BaseUrl;

            _logger.LogDebugIf(Debug, "[DEPT-CLIENT] GetList {Url}", url);
            return _http.GetAsync<List<DepartmentItemViewModel>>(url, ct);
        }

        public Task<ApiResponse<DepartmentEditViewModel>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return _http.GetAsync<DepartmentEditViewModel>($"{BaseUrl}/{id}", ct);
        }

        public Task<ApiResponse<object>> CreateAsync(DepartmentEditViewModel model, CancellationToken ct = default)
        {
            _logger.LogDebugIf(Debug, "[DEPT-CLIENT] Create {Code}", model.DeptCode);
            return _http.PostAsync<object>(BaseUrl, model, ct);
        }

        public Task<ApiResponse<object>> UpdateAsync(DepartmentEditViewModel model, CancellationToken ct = default)
        {
            _logger.LogDebugIf(Debug, "[DEPT-CLIENT] Update {Id}", model.Id);
            return _http.PutAsync<object>(BaseUrl, model, ct);
        }

        public Task<ApiResponse<object>> ToggleAsync(int id, CancellationToken ct = default)
        {
            // IHttpClientWithAuth không có PatchAsync sẵn -> dùng PutAsync vào endpoint toggle
            return _http.PutAsync<object>($"{BaseUrl}/{id}/toggle", new { }, ct);
        }

        public Task<ApiResponse<object>> DeleteAsync(int id, CancellationToken ct = default)
        {
            return _http.DeleteAsync<object>($"{BaseUrl}/{id}", ct);
        }
    }
}