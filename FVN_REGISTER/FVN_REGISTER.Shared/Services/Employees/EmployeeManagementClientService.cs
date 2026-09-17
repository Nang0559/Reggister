using FVN_REGISTER.Contract.Dtos.Employees;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;

namespace FVN_REGISTER.Shared.Services.Employees
{
    public sealed class EmployeeManagementClientService : IEmployeeManagementClientService
    {
        private const string Base = "api/EmployeeManagement";
        private readonly IHttpClientWithAuth _http;

        public EmployeeManagementClientService(IHttpClientWithAuth http)
        {
            _http = http;
        }

        public Task<ApiResponse<List<EmployeeDeptTreeDto>>> GetTreeAsync(
            string? searchTerm = null,
            string? deptCode = null,
            CancellationToken ct = default)
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(searchTerm))
                query.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
            if (!string.IsNullOrWhiteSpace(deptCode))
                query.Add($"deptCode={Uri.EscapeDataString(deptCode)}");

            var url = query.Count == 0
                ? $"{Base}/tree"
                : $"{Base}/tree?{string.Join("&", query)}";

            return _http.GetAsync<List<EmployeeDeptTreeDto>>(url, ct);
        }

        public Task<ApiResponse<EmployeeCardDto>> GetByIdAsync(
            int id,
            CancellationToken ct = default)
            => _http.GetAsync<EmployeeCardDto>($"{Base}/{id}", ct);

        public Task<ApiResponse<EmployeeOtSummaryDto>> GetOtSummaryAsync(
            string employeeCode,
            int? year = null,
            CancellationToken ct = default)
        {
            var url = year.HasValue
                ? $"{Base}/ot-summary/{Uri.EscapeDataString(employeeCode)}?year={year.Value}"
                : $"{Base}/ot-summary/{Uri.EscapeDataString(employeeCode)}";
            return _http.GetAsync<EmployeeOtSummaryDto>(url, ct);
        }

        public Task<ApiResponse<object>> CreateAsync(
            EmployeeUpsertDto model,
            CancellationToken ct = default)
            => _http.PostAsync<object>(Base, model, ct);

        public Task<ApiResponse<object>> UpdateAsync(
            int id,
            EmployeeUpsertDto model,
            CancellationToken ct = default)
            => _http.PutAsync<object>($"{Base}/{id}", model, ct);

        public Task<ApiResponse<object>> ToggleAsync(
            int id,
            CancellationToken ct = default)
            => _http.PatchAsync<object>($"{Base}/{id}/toggle", new { }, ct);

        public Task<ApiResponse<object>> DeleteAsync(
            int id,
            CancellationToken ct = default)
            => _http.DeleteAsync<object>($"{Base}/{id}", ct);
    }
}
