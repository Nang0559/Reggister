using AutoMapper;
using FVN_REGISTER.API.Services.Approvals;
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Contract.Interfaces.Approvals;
using FVN_REGISTER.Contract.Interfaces.Leaves;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.Models.ApprovalModels;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Contract.ViewModels.Approvals;
using FVN_REGISTER.Contract.ViewModels.Leaves;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    /// <summary>
    /// API cho nghiệp vụ đăng ký/duyệt nghỉ phép.
    /// Toàn bộ logic duyệt (build hierarchy, xử lý quyết định, tính trạng thái tổng)
    /// nằm trong LeaveService + ApprovalEngine/ApprovalProvider — Controller chỉ
    /// nhận request, lấy UserInfo, gọi Service, trả kết quả. Không tự query DB
    /// hay tự xử lý logic nghiệp vụ ở đây.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveDaysController : BaseApiController
    {
        private readonly ILeaveService _leaveService;

        public LeaveDaysController(
            ILeaveService leaveService,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<LeaveDaysController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            _leaveService = leaveService;
            // KHÔNG inject ApprovalListService<LeaveRequestViewModel> trực tiếp ở đây —
            // LeaveService đã có _pendingService riêng và expose qua
            // GetMyPendingAsync/GetMyPendingSummaryAsync. Controller chỉ nên có
            // 1 cửa vào duy nhất (ILeaveService) để tránh 2 chỗ cùng đọc
            // F03ApprovalSteps theo 2 cách khác nhau.
        }

        // ══════════════════════════════════════════════════════════════════
        // COMMANDS — Nhân viên
        // ══════════════════════════════════════════════════════════════════

        [HttpPost("create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateLeaveRequestModel model, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            await LogActionAsync($"Đăng ký nghỉ: {model.StartDate:dd/MM} - {model.EndDate:dd/MM}");

            var result = await _leaveService.CreateLeaveAsync(model, UserInfo, ct);
            return HandleResult(result);
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(
            int id, [FromBody] CancelLeaveBody body, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            var result = await _leaveService.CancelAsync(id, body.Reason, UserInfo, ct);
            return HandleResult(result);
        }

        [HttpPost("cancel-detail/{detailId:int}")]
        public async Task<IActionResult> CancelDetail(
            int detailId, [FromBody] CancelDetailRequest request, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            var result = await _leaveService.CancelDetailAsync(
                detailId, request.Reason, UserInfo, ct);
            return HandleResult(result);
        }

        // ══════════════════════════════════════════════════════════════════
        // COMMANDS — Approver
        // ══════════════════════════════════════════════════════════════════

        [HttpPost("approve")]
        public async Task<IActionResult> Approve(
            [FromBody] ApproveRequest req, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            await LogActionAsync($"Duyệt đơn ID: {string.Join(",", req.Ids)}");

            var result = await _leaveService.ApproveAsync(
                req.Ids, req.Level, UserInfo, req.Comment ?? "", ct);
            return HandleResult(result);
        }

        [HttpPost("reject")]
        public async Task<IActionResult> Reject(
            [FromBody] ApproveRequest req, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            // Lý do từ chối là bắt buộc — chặn ngay ở Controller để tránh
            // round-trip không cần thiết tới Engine (Engine không tự validate
            // comment rỗng, đó là input contract của API, không phải business rule).
            if (string.IsNullOrWhiteSpace(req.Comment))
                return BadRequest(ApiResponse<object>.Fail("Lý do từ chối không được để trống."));

            await LogActionAsync($"Từ chối đơn ID: {string.Join(",", req.Ids)}");

            var result = await _leaveService.RejectAsync(
                req.Ids, req.Level, UserInfo, req.Comment!, ct);
            return HandleResult(result);
        }

        // ══════════════════════════════════════════════════════════════════
        // QUERIES — Approver (pending)
        // ══════════════════════════════════════════════════════════════════

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending(CancellationToken ct)
        {
            if (UserInfo?.Email == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            await LogActionAsync("Xem danh sách chờ duyệt");

            // GetMyPendingAsync nhận approverEmail — KHÔNG phải EmployeeCode.
            // F03ApprovalSteps.ApproverEmail lưu email, không lưu EmployeeCode.
            var result = await _leaveService.GetMyPendingAsync(UserInfo.Email, ct);
            return HandleResult(result);
        }

        [HttpGet("pending-summary")]
        public async Task<IActionResult> GetPendingSummary(CancellationToken ct)
        {
            if (UserInfo?.Email == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var result = await _leaveService.GetMyPendingSummaryAsync(UserInfo.Email, ct);
            return HandleResult(result);
        }

        // ══════════════════════════════════════════════════════════════════
        // QUERIES — Nhân viên
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Số dư phép theo năm. Dùng GetBalanceAsync(year, user) — đường mới
        /// đi qua IHistoryHandler, trả BalanceSummaryDto đầy đủ hơn
        /// LeaveBalanceViewModel cũ. Nếu FE cũ còn cần đúng shape
        /// LeaveBalanceViewModel, gọi GetLeaveBalanceAsync thay thế —
        /// không gọi cả 2 cho cùng 1 màn hình để tránh lệch số liệu.
        /// </summary>
        [HttpGet("balance/{year:int}")]
        public async Task<IActionResult> GetBalance(int year, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            var result = await _leaveService.GetBalanceAsync(year, UserInfo, ct);
            return HandleResult(result);
        }

        [HttpGet("details/{id:int}")]
        public async Task<IActionResult> GetDetails(int id, CancellationToken ct)
        {
            var result = await _leaveService.GetDetailsAsync(id, ct);
            return HandleResult(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(
            [FromQuery] int? year,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            // LeaveService.GetHistoryAsync nhận HistoryFilterDto + CurrentUser,
            // không phải (empCode, year, status) rời rạc — Controller chỉ
            // gom query string thành filter object, không tự query DB.
            var filter = new HistoryFilterDto
            {
                Year = year,
                Status = status,
                Page = page,
                PageSize = pageSize
            };

            var result = await _leaveService.GetHistoryAsync(filter, UserInfo, ct);
            return HandleResult(result);
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent(
            [FromQuery] int limit = 5, CancellationToken ct = default)
        {
            if (UserInfo?.EmployeeCode == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            var result = await _leaveService.GetRecentRequestsAsync(
                UserInfo.EmployeeCode, limit, ct);
            return HandleResult(result);
        }
    }

    public record CancelLeaveBody(string Reason);

    public class CancelDetailRequest
    {
        public string Reason { get; set; } = "";
    }
}