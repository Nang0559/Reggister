using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Requests.Leaves;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Shared.Services.Leaves
{
    public class LeaveCreateClientService : ILeaveCreateClientService
    {
        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<LeaveCreateClientService> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;

        private bool Debug => _options.CurrentValue.Enabled;

        public LeaveCreateClientService(
            IHttpClientWithAuth http,
            ILogger<LeaveCreateClientService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _http = http;
            _logger = logger;
            _options = options;
        }

        public async Task<ApiResponse<SystemMasterDataDto>> GetCombinedDataAsync(
            string empCode,
            string deptCode,
            string cvCode,
            int year,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[LEAVE_CLIENT] GetCombinedData emp={Emp} year={Year}", empCode, year);

                var url = $"api/leavecalendar/data?empCode={Uri.EscapeDataString(empCode)}" +
                          $"&deptCode={Uri.EscapeDataString(deptCode)}" +
                          $"&cvCode={Uri.EscapeDataString(cvCode)}" +
                          $"&year={year}";

                var result = await _http.GetAsync<SystemMasterDataDto>(url, ct);

                _logger.LogDebugIf(Debug, "[LEAVE_CLIENT] GetCombinedData ok={Ok}", result.IsSuccess);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LEAVE_CLIENT] GetCombinedData error");
                return ApiResponse<SystemMasterDataDto>.Fail("Không thể tải dữ liệu lịch");
            }
        }

        public async Task<ApiResponse<object>> CreateLeaveAsync(
            LeaveRequestUpsertDto model,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[LEAVE_CLIENT] CreateLeave start={Start}", model.StartDate);
                return await _http.PostAsync<object>("api/leavedays/create", model, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LEAVE_CLIENT] CreateLeave error");
                return ApiResponse<object>.Fail("Không thể gửi đơn nghỉ");
            }
        }

        public async Task<ApiResponse<object>> CancelLeaveAsync(
            int leaveId,
            string reason,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[LEAVE_CLIENT] Cancel id={Id}", leaveId);
                return await _http.PostAsync<object>($"api/leavedays/{leaveId}/cancel", new { reason }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LEAVE_CLIENT] Cancel error");
                return ApiResponse<object>.Fail("Không thể hủy đơn");
            }
        }

        public async Task<ApiResponse<object>> CancelDetailAsync(
            int detailId,
            string reason,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[LEAVE_CLIENT] CancelDetail detailId={Id}", detailId);
                return await _http.PostAsync<object>($"api/leavedays/cancel-detail/{detailId}", new { reason }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LEAVE_CLIENT] CancelDetail error");
                return ApiResponse<object>.Fail("Không thể hủy ngày nghỉ");
            }
        }
    }
}
