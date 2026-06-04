using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Contract.ViewModels.OT;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Shared.Handlers;
using FVN_REGISTER.Shared.Utils.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Shared.Services.OTs
{
    public class OTClientService : IOTClientService
    {
        private readonly IHttpClientWithAuth _http;
        private readonly ILogger<OTClientService> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;

        private bool Debug => _options.CurrentValue.Enabled;
        private const string BASE = "api/OT";

        public OTClientService(
            IHttpClientWithAuth http,
            ILogger<OTClientService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _http = http;
            _logger = logger;
            _options = options;
        }

        // ============================================================
        // COMMANDS
        // ============================================================

        public async Task<ApiResponse<object>> CreateOTRequestAsync(
            CreateOTRequestModel model,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[OT_CLIENT] Create date={Date}", model.OTDate);
                // Controller: [HttpPost("create")] → POST api/OT/create
                return await _http.PostAsync<object>($"{BASE}/create", model, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] Create ERROR");
                return ApiResponse<object>.Fail("Không thể tạo đơn OT.");
            }
        }

        public async Task<ApiResponse<object>> ApproveAsync(
            OTApproveRequest request,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug,
                    "[OT_CLIENT] Approve level={Level}", request.Level);
                return await _http.PostAsync<object>($"{BASE}/approve", request, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] Approve ERROR");
                return ApiResponse<object>.Fail("Không thể phê duyệt.");
            }
        }

        public async Task<ApiResponse<object>> RejectAsync(
            OTApproveRequest request,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug,
                    "[OT_CLIENT] Reject level={Level}", request.Level);
                return await _http.PostAsync<object>($"{BASE}/reject", request, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] Reject ERROR");
                return ApiResponse<object>.Fail("Không thể từ chối.");
            }
        }

        public async Task<ApiResponse<object>> CancelAsync(
            int otRequestId,
            string? reason,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[OT_CLIENT] Cancel OT {Id}", otRequestId);
                // Controller nhận record CancelOTRequest(string? Reason)
                return await _http.PostAsync<object>(
                    $"{BASE}/cancel/{otRequestId}",
                    new { Reason = reason },
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] Cancel ERROR");
                return ApiResponse<object>.Fail("Không thể hủy đơn OT.");
            }
        }

        // ============================================================
        // QUERIES
        // ============================================================

        public async Task<ApiResponse<CombinedOTViewModel>> GetCombinedDataAsync(
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[OT_CLIENT] GetCombinedData");
                return await _http.GetAsync<CombinedOTViewModel>(
                    $"{BASE}/combined-data", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetCombinedData ERROR");
                return ApiResponse<CombinedOTViewModel>.Fail("Không tải được dữ liệu trang OT.");
            }
        }

        public async Task<ApiResponse<OTBalanceDto>> GetOTBalanceAsync(
            int year,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[OT_CLIENT] GetBalance year={Year}", year);
                // Controller: GET api/OT/balance/{year}
                return await _http.GetAsync<OTBalanceDto>(
                    $"{BASE}/balance/{year}", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetBalance ERROR");
                return ApiResponse<OTBalanceDto>.Fail("Không tải được số dư OT.");
            }
        }

        public async Task<ApiResponse<OTRequestViewModel>> GetOTDetailAsync(
            int otRequestId,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[OT_CLIENT] GetDetail id={Id}", otRequestId);
                // Controller: GET api/OT/detail/{id}
                return await _http.GetAsync<OTRequestViewModel>(
                    $"{BASE}/detail/{otRequestId}", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetDetail ERROR");
                return ApiResponse<OTRequestViewModel>.Fail("Không tải được chi tiết đơn OT.");
            }
        }

        public async Task<ApiResponse<List<OTRequestViewModel>>> GetRecentOTRequestsAsync(
            int limit = 10,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[OT_CLIENT] GetRecent limit={L}", limit);
                return await _http.GetAsync<List<OTRequestViewModel>>(
                    $"{BASE}/recent?limit={limit}", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetRecent ERROR");
                return ApiResponse<List<OTRequestViewModel>>.Fail("Không tải được lịch sử OT.");
            }
        }

        public async Task<ApiResponse<List<OTRequestViewModel>>> GetPendingApprovalsAsync(
            int level = 0,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[OT_CLIENT] GetPending level={L}", level);
                return await _http.GetAsync<List<OTRequestViewModel>>(
                    $"{BASE}/pending?level={level}", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetPending ERROR");
                return ApiResponse<List<OTRequestViewModel>>.Fail("Không tải được danh sách chờ duyệt.");
            }
        }

        public async Task<ApiResponse<PaginationResult<OTRequestViewModel>>> GetPagedOTRequestsAsync(
            string? deptCode = null,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            try
            {
                var url = $"{BASE}/list?page={page}&pageSize={pageSize}";
                if (!string.IsNullOrEmpty(deptCode)) url += $"&deptCode={deptCode}";
                if (!string.IsNullOrEmpty(status)) url += $"&status={status}";
                if (fromDate.HasValue) url += $"&fromDate={fromDate:yyyy-MM-dd}";
                if (toDate.HasValue) url += $"&toDate={toDate:yyyy-MM-dd}";

                return await _http.GetPagedAsync<OTRequestViewModel>(url, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetPaged ERROR");
                return ApiResponse<PaginationResult<OTRequestViewModel>>.Fail(
                    "Không tải được danh sách đơn OT.");
            }
        }

        public async Task<ApiResponse<OTDashboardViewModel>> GetOTDashboardAsync(
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[OT_CLIENT] GetDashboard");
                return await _http.GetAsync<OTDashboardViewModel>(
                    $"{BASE}/dashboard", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetDashboard ERROR");
                return ApiResponse<OTDashboardViewModel>.Fail("Không tải được dashboard OT.");
            }
        }

        public async Task<ApiResponse<OTValidationResultDto>> ValidateOTHoursAsync(
            string employeeCode,
            DateTime otDate,
            decimal hours,
            string otType,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug,
                    "[OT_CLIENT] Validate emp={E} hours={H}", employeeCode, hours);
                // Controller nhận record ValidateOTRequest(EmployeeCode, OTDate, Hours, OTType)
                return await _http.PostAsync<OTValidationResultDto>($"{BASE}/validate", new
                {
                    EmployeeCode = employeeCode,
                    OTDate = otDate,
                    Hours = hours,
                    OTType = otType
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] Validate ERROR");
                return ApiResponse<OTValidationResultDto>.Fail("Không thể kiểm tra giờ OT.");
            }
        }
    }
}

