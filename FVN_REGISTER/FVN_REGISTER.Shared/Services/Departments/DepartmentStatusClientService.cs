using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Shared.Services.Departments
{
    public sealed class DepartmentStatusClientService : IDepartmentStatusClientService
    {
        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<DepartmentStatusClientService> _logger;

        public DepartmentStatusClientService(
            IHttpClientWithAuth http,
            ILogger<DepartmentStatusClientService> logger)
        {
            _http = http;
            _logger = logger;
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

                var result = await _http.GetAsync<List<DepartmentStatusDto>>(url, ct);
                if (!result.IsSuccess)
                    _logger.LogWarning(
                        "[DEPT_STATUS_CLIENT] GetAll failed | Status={Status} | Msg={Msg}",
                        result.StatusCode,
                        result.Message);
                return result;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
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
                    ? $"api/department-status/{Uri.EscapeDataString(deptCode)}?date={date.Value:yyyy-MM-dd}"
                    : $"api/department-status/{Uri.EscapeDataString(deptCode)}";

                var result = await _http.GetAsync<DepartmentStatusDto>(url, ct);
                if (!result.IsSuccess)
                    _logger.LogWarning(
                        "[DEPT_STATUS_CLIENT] GetByDept failed | Status={Status} | Msg={Msg}",
                        result.StatusCode,
                        result.Message);
                return result;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DEPT_STATUS_CLIENT] GetByDept exception");
                return ApiResponse<DepartmentStatusDto>.Fail("Không thể tải dữ liệu phòng ban.");
            }
        }
    }
}
