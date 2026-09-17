
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;
using FVN_REGISTER.Core.Logging;
using Microsoft.Extensions.Options;

using FVN_REGISTER.Contract.Responses;


namespace FVN_REGISTER.Shared.Services.Approvals
{
    public class ApproverClientService : IApproverClientService
    {
        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<ApproverClientService> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;

        private bool Debug => _options.CurrentValue.Enabled;

        public ApproverClientService(
            IHttpClientWithAuth http,
            ILogger<ApproverClientService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _http = http;
            _logger = logger;
            _options = options;
        }

        // ================= TREE =================
        public async Task<ApiResponse<List<ApproverTreeNodeViewModel>>> GetTreeAsync(
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[APPROVER] GetTree start");

                var result = await _http.GetAsync<List<ApproverTreeNodeViewModel>>(
                    "api/Approver/tree", ct);

                if (!result.IsSuccess)
                    _logger.LogWarnIf(Debug, "[APPROVER] GetTree failed | {Msg}", result.Message);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[APPROVER] GetTree exception");
                return ApiResponse<List<ApproverTreeNodeViewModel>>.Fail("Không thể tải danh sách approver.");
            }
        }

        // ================= LIST =================
        public async Task<ApiResponse<List<ApproverViewModel>>> GetListAsync(
            string? deptCode,
            int? level,
            string? requestType,
            CancellationToken ct = default)
        {
            try
            {
                var query = BuildQuery(new()
                {
                    ["deptCode"] = deptCode,
                    ["level"] = level?.ToString(),
                    ["requestType"] = requestType
                });

                _logger.LogDebugIf(Debug, "[APPROVER] GetList: {Query}", query);

                return await _http.GetAsync<List<ApproverViewModel>>(
                    $"api/Approver/list{query}", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[APPROVER] GetList exception");
                return ApiResponse<List<ApproverViewModel>>.Fail("Không thể tải danh sách.");
            }
        }

        // ================= DEPARTMENTS =================
        public async Task<ApiResponse<List<DepartmentViewModel>>> GetDepartmentsAsync(
            CancellationToken ct = default)
        {
            try
            {
                return await _http.GetAsync<List<DepartmentViewModel>>(
                    "api/Approver/departments", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[APPROVER] GetDepartments exception");
                return ApiResponse<List<DepartmentViewModel>>.Fail("Không thể tải danh sách phòng ban.");
            }
        }

        // ================= EMPLOYEES =================
        public async Task<ApiResponse<List<EmployeeSelectViewModel>>> GetEmployeesAsync(
            string? deptCode,
            CancellationToken ct = default)
        {
            try
            {
                var query = BuildQuery(new() { ["deptCode"] = deptCode });

                return await _http.GetAsync<List<EmployeeSelectViewModel>>(
                    $"api/Approver/employees{query}", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[APPROVER] GetEmployees exception");
                return ApiResponse<List<EmployeeSelectViewModel>>.Fail("Không thể tải danh sách nhân viên.");
            }
        }

        // ================= CREATE =================
        public async Task<ApiResponse<object>> CreateAsync(
            ApproverViewModel model,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug,
                    "[APPROVER] Create: {Name} Type={Type}",
                    model.ApproveLevelName, model.RequestType);

                return await _http.PostAsync<object>("api/Approver", model, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[APPROVER] Create exception");
                return ApiResponse<object>.Fail("Lỗi khi thêm approver.");
            }
        }

        // ================= UPDATE =================
        public async Task<ApiResponse<object>> UpdateAsync(
            int id,
            ApproverViewModel model,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[APPROVER] Update Id={Id}", id);

                return await _http.PutAsync<object>($"api/Approver/{id}", model, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[APPROVER] Update exception Id={Id}", id);
                return ApiResponse<object>.Fail("Lỗi khi cập nhật approver.");
            }
        }

        // ================= DELETE =================
        public async Task<ApiResponse<object>> DeleteAsync(
            int id,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[APPROVER] Delete Id={Id}", id);

                return await _http.DeleteAsync<object>($"api/Approver/{id}", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[APPROVER] Delete exception Id={Id}", id);
                return ApiResponse<object>.Fail("Lỗi khi xóa approver.");
            }
        }

        // ================= TOGGLE =================
        public async Task<ApiResponse<object>> ToggleAsync(
            int id,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[APPROVER] Toggle Id={Id}", id);

                // IHttpClientWithAuth chưa có PatchAsync sẵn — dùng PostAsync tới đúng route PATCH
                // hoặc bổ sung PatchAsync vào IHttpClientWithAuth (xem mục 4 dưới)
                return await _http.PatchAsync<object>($"api/Approver/{id}/toggle", new { }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[APPROVER] Toggle exception Id={Id}", id);
                return ApiResponse<object>.Fail("Lỗi khi đổi trạng thái approver.");
            }
        }

        // ================= HELPER =================
        private static string BuildQuery(Dictionary<string, string?> parameters)
        {
            var parts = parameters
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}")
                .ToList();

            return parts.Count > 0 ? "?" + string.Join("&", parts) : "";
        }
    }
}
