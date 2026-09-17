using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Requests.Leaves;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Shared.Services.Leaves
{
    public class LeaveCreateClientService : ILeaveCreateClientService
    {
        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<LeaveCreateClientService> _logger;

        public LeaveCreateClientService(
            IHttpClientWithAuth http,
            ILogger<LeaveCreateClientService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<ApiResponse<LeaveCalendarDataDto>> GetCombinedDataAsync(
            string empCode,
            string deptCode,
            string cvCode,
            int year,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebug(
                    "[LEAVE_CLIENT] GetCombinedData emp={Emp} year={Year}",
                    empCode,
                    year);

                var url = $"api/leavecalendar/data?empCode={Uri.EscapeDataString(empCode)}" +
                          $"&deptCode={Uri.EscapeDataString(deptCode)}" +
                          $"&cvCode={Uri.EscapeDataString(cvCode)}" +
                          $"&year={year}";

                var result = await _http.GetAsync<LeaveCalendarDataDto>(url, ct);

                _logger.LogDebug(
                    "[LEAVE_CLIENT] GetCombinedData success={Success}",
                    result.IsSuccess);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LEAVE_CLIENT] GetCombinedData error");
                return ApiResponse<LeaveCalendarDataDto>.Fail("Không thể tải dữ liệu lịch");
            }
        }

        public async Task<ApiResponse<object>> CreateLeaveAsync(
            LeaveRequestUpsertDto model,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebug(
                    "[LEAVE_CLIENT] CreateLeave start={Start}",
                    model.StartDate);

                return await _http.PostAsync<object>(
                    "api/leavedays/create",
                    model,
                    ct);
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
                _logger.LogDebug(
                    "[LEAVE_CLIENT] Cancel id={Id}",
                    leaveId);

                return await _http.PostAsync<object>(
                    $"api/leavedays/{leaveId}/cancel",
                    new { reason },
                    ct);
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
                _logger.LogDebug(
                    "[LEAVE_CLIENT] CancelDetail detailId={Id}",
                    detailId);

                return await _http.PostAsync<object>(
                    $"api/leavedays/cancel-detail/{detailId}",
                    new { reason },
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LEAVE_CLIENT] CancelDetail error");
                return ApiResponse<object>.Fail("Không thể hủy ngày nghỉ");
            }
        }
    }
}
