using FVN_REGISTER.Contract.Dtos.Depts;

using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Departments
{
    public class DepartmentStatusClientService : IDepartmentStatusClientService
    {
        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<DepartmentStatusClientService> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;

        private bool Debug => _options.CurrentValue.Enabled;

        public DepartmentStatusClientService(
            IHttpClientWithAuth http,
            ILogger<DepartmentStatusClientService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _http = http;
            _logger = logger;
            _options = options;
        }

        public async Task<ApiResponse<List<DepartmentStatusDto>>> GetAllAsync(
            DateTime? date = null,
            CancellationToken ct = default)
        {
            try
            {
                var url = date.HasValue
                    ? $"api/department-status/all?date={date.Value:yyyy-MM-dd}"
                    : "api/department-status/all";

                _logger.LogDebugIf(Debug, "[DEPT_STATUS_CLIENT] GetAll → {Url}", url);

                var result = await _http.GetAsync<List<DepartmentStatusDto>>(url, ct);

                if (!result.IsSuccess)
                    _logger.LogWarnIf(Debug,
                        "[DEPT_STATUS_CLIENT] GetAll failed | Status={Status} | Msg={Msg}",
                        result.StatusCode, result.Message);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DEPT_STATUS_CLIENT] GetAll exception");
                return ApiResponse<List<DepartmentStatusDto>>.Fail("Không thể tải dữ liệu phòng ban.");
            }
        }

        public async Task<ApiResponse<DepartmentStatusDto>> GetByDeptAsync(
            string deptCode,
            DateTime? date = null,
            CancellationToken ct = default)
        {
            try
            {
                var url = date.HasValue
                    ? $"api/department-status/{deptCode}?date={date.Value:yyyy-MM-dd}"
                    : $"api/department-status/{deptCode}";

                _logger.LogDebugIf(Debug,
                    "[DEPT_STATUS_CLIENT] GetByDept → {Url}", url);

                var result = await _http.GetAsync<DepartmentStatusDto>(url, ct);

                if (!result.IsSuccess)
                    _logger.LogWarnIf(Debug,
                        "[DEPT_STATUS_CLIENT] GetByDept failed | Status={Status} | Msg={Msg}",
                        result.StatusCode, result.Message);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DEPT_STATUS_CLIENT] GetByDept exception");
                return ApiResponse<DepartmentStatusDto>.Fail("Không thể tải dữ liệu phòng ban.");
            }
        }
    }
}
