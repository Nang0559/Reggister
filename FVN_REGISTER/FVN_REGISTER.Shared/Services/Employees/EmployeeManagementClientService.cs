
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace FVN_REGISTER.Shared.Services.Employees
{
    public class EmployeeManagementClientService : IEmployeeManagementClientService
    {
        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<EmployeeManagementClientService> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;
        private bool Debug => _options.CurrentValue.Enabled;

        private const string Base = "api/employeemanagement";

        public EmployeeManagementClientService(
            IHttpClientWithAuth http,
            ILogger<EmployeeManagementClientService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _http = http;
            _logger = logger;
            _options = options;
        }

        public Task<ApiResponse<List<EmployeeDeptTreeViewModel>>> GetTreeAsync(
            string? searchTerm = null,
            string? deptCode = null,
            CancellationToken ct = default)
        {
            var qs = new List<string>();
            if (!string.IsNullOrEmpty(searchTerm))
                qs.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
            if (!string.IsNullOrEmpty(deptCode))
                qs.Add($"deptCode={Uri.EscapeDataString(deptCode)}");

            var url = qs.Any()
                ? $"{Base}/tree?{string.Join("&", qs)}"
                : $"{Base}/tree";

            return _http.GetAsync<List<EmployeeDeptTreeViewModel>>(url, ct);
        }

        public Task<ApiResponse<EmployeeCardViewModel>> GetByIdAsync(
            int id, CancellationToken ct = default)
            => _http.GetAsync<EmployeeCardViewModel>($"{Base}/{id}", ct);

        public Task<ApiResponse<List<DepartmentViewModel>>> GetCvListAsync(
            CancellationToken ct = default)
            => _http.GetAsync<List<DepartmentViewModel>>($"{Base}/cv-list", ct);

        public Task<ApiResponse<EmployeeOtSummaryViewModel>> GetOtSummaryAsync(
            string employeeCode,
            int? year = null,
            CancellationToken ct = default)
        {
            var url = year.HasValue
                ? $"{Base}/ot-summary/{employeeCode}?year={year}"
                : $"{Base}/ot-summary/{employeeCode}";
            return _http.GetAsync<EmployeeOtSummaryViewModel>(url, ct);
        }

        public Task<ApiResponse<object>> CreateAsync(
            EmployeeFormViewModel model, CancellationToken ct = default)
            => _http.PostAsync<object>(Base, model, ct);

        public Task<ApiResponse<object>> UpdateAsync(
            int id, EmployeeFormViewModel model, CancellationToken ct = default)
            => _http.PutAsync<object>($"{Base}/{id}", model, ct);

        public Task<ApiResponse<object>> ToggleAsync(
            int id, CancellationToken ct = default)
            => _http.PutAsync<object>($"{Base}/{id}/toggle", new { }, ct);

        public Task<ApiResponse<object>> DeleteAsync(
            int id, CancellationToken ct = default)
            => _http.DeleteAsync<object>($"{Base}/{id}", ct);
    }
}
