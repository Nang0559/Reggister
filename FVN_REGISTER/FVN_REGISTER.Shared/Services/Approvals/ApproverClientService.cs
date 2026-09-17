using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;

namespace FVN_REGISTER.Shared.Services.Approvals
{
    public sealed class ApproverClientService : IApproverClientService
    {
        private readonly IHttpClientWithAuth _http;

        public ApproverClientService(IHttpClientWithAuth http)
        {
            _http = http;
        }

        public Task<ApiResponse<List<ApproverTreeNodeDto>>> GetTreeAsync(
            CancellationToken ct = default)
            => _http.GetAsync<List<ApproverTreeNodeDto>>("api/Approver/tree", ct);

        public Task<ApiResponse<List<ApproverDto>>> GetListAsync(
            string? deptCode,
            int? level,
            string? requestType,
            CancellationToken ct = default)
        {
            var query = BuildQuery(new Dictionary<string, string?>
            {
                ["deptCode"] = deptCode,
                ["level"] = level?.ToString(),
                ["requestType"] = requestType
            });

            return _http.GetAsync<List<ApproverDto>>($"api/Approver/list{query}", ct);
        }

        public Task<ApiResponse<List<DepartmentDto>>> GetDepartmentsAsync(
            CancellationToken ct = default)
            => _http.GetAsync<List<DepartmentDto>>("api/Approver/departments", ct);

        public Task<ApiResponse<List<EmployeeSelectDto>>> GetEmployeesAsync(
            string? deptCode,
            CancellationToken ct = default)
        {
            var query = BuildQuery(new Dictionary<string, string?>
            {
                ["deptCode"] = deptCode
            });

            return _http.GetAsync<List<EmployeeSelectDto>>(
                $"api/Approver/employees{query}", ct);
        }

        public Task<ApiResponse<object>> CreateAsync(
            ApproverDto model,
            CancellationToken ct = default)
            => _http.PostAsync<object>("api/Approver", model, ct);

        public Task<ApiResponse<object>> UpdateAsync(
            int id,
            ApproverDto model,
            CancellationToken ct = default)
            => _http.PutAsync<object>($"api/Approver/{id}", model, ct);

        public Task<ApiResponse<object>> DeleteAsync(
            int id,
            CancellationToken ct = default)
            => _http.DeleteAsync<object>($"api/Approver/{id}", ct);

        public Task<ApiResponse<object>> ToggleAsync(
            int id,
            CancellationToken ct = default)
            => _http.PatchAsync<object>($"api/Approver/{id}/toggle", new { }, ct);

        private static string BuildQuery(Dictionary<string, string?> parameters)
        {
            var parts = parameters
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                .Select(kv =>
                    $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}")
                .ToList();

            return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
        }
    }
}
