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

namespace FVN_REGISTER.Shared.Services.OT
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

        // ===== COMMANDS =====

        public async Task<ApiResponse<object>> CreateOTRequestAsync(
            CreateOTRequestModel model,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[OT_CLIENT] CreateOTRequest");
                return await _http.PostAsync<object>($"{BASE}/create", model, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] CreateOTRequest ERROR");
                return ApiResponse<object>.Fail("Không thể tạo đơn OT.");
            }
        }

        public async Task<ApiResponse<object>> ApproveAsync(
            OTApproveRequest request,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[OT_CLIENT] Approve level={Level}", request.Level);
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
                _logger.LogDebugIf(Debug, "[OT_CLIENT] Reject level={Level}", request.Level);
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
            string reason,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogDebugIf(Debug, "[OT_CLIENT] Cancel OT {Id}", otRequestId);
                return await _http.PostAsync<object>($"{BASE}/cancel/{otRequestId}", new { Reason = reason }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] Cancel ERROR");
                return ApiResponse<object>.Fail("Không thể hủy đơn OT.");
            }
        }

        // ===== QUERIES =====

        public async Task<ApiResponse<CreateOTRequestModel>> GetCombinedDataAsync(
            CancellationToken ct = default)
        {
            try
            {
                return await _http.GetAsync<CreateOTRequestModel>($"{BASE}/combined-data", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetCombinedData ERROR");
                return ApiResponse<CreateOTRequestModel>.Fail("Không tải được dữ liệu.");
            }
        }

        public async Task<ApiResponse<OTBalanceDto>> GetOTBalanceAsync(
            int year,
            CancellationToken ct = default)
        {
            try
            {
                return await _http.GetAsync<OTBalanceDto>($"{BASE}/balance/{year}", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetOTBalance ERROR");
                return ApiResponse<OTBalanceDto>.Fail("Không tải được số dư OT.");
            }
        }

        public async Task<ApiResponse<List<OTRequestViewModel>>> GetRecentOTRequestsAsync(
            int limit = 10,
            CancellationToken ct = default)
        {
            try
            {
                return await _http.GetAsync<List<OTRequestViewModel>>($"{BASE}/recent?limit={limit}", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetRecent ERROR");
                return ApiResponse<List<OTRequestViewModel>>.Fail("Không tải được lịch sử.");
            }
        }

        public async Task<ApiResponse<OTRequestViewModel>> GetOTDetailAsync(
            int otRequestId,
            CancellationToken ct = default)
        {
            try
            {
                return await _http.GetAsync<OTRequestViewModel>($"{BASE}/detail/{otRequestId}", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetOTDetail ERROR");
                return ApiResponse<OTRequestViewModel>.Fail("Không tải được chi tiết.");
            }
        }

        public async Task<ApiResponse<List<OTRequestViewModel>>> GetPendingApprovalsAsync(
            int level = 0,
            CancellationToken ct = default)
        {
            try
            {
                return await _http.GetAsync<List<OTRequestViewModel>>($"{BASE}/pending?level={level}", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OT_CLIENT] GetPendingApprovals ERROR");
                return ApiResponse<List<OTRequestViewModel>>.Fail("Không tải được danh sách chờ duyệt.");
            }
        }

        public async Task<ApiResponse<PaginationResult<OTRequestViewModel>>> GetPagedOTRequestsAsync(
            string? deptCode,
            string? status,
            DateTime? fromDate,
            DateTime? toDate,
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
                return ApiResponse<PaginationResult<OTRequestViewModel>>.Fail("Không tải được danh sách.");
            }
        }

        public async Task<ApiResponse<OTDashboardViewModel>> GetOTDashboardAsync(
            CancellationToken ct = default)
        {
            try
            {
                return await _http.GetAsync<OTDashboardViewModel>($"{BASE}/dashboard", ct);
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