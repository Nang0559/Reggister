using AutoMapper;
using FVN_REGISTER.API.Services.Leaves;
using FVN_REGISTER.Contract.Interfaces.Leaves;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveDaysController : BaseApiController
    {
        private readonly ILeaveService _leaveService;
        private readonly ILeaveQueryService _leaveQueryService;
        public LeaveDaysController(
          
            ILeaveService leaveService,
             ILeaveQueryService leaveQueryService,
            ICurrentUserService currentUser,
            IUserLogService userLog,
            IMapper mapper,
            ILogger<LeaveDaysController> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(currentUser, userLog, mapper, logger, options)
        {
            
            _leaveService = leaveService;
            _leaveQueryService = leaveQueryService;
        }
        
        // ==========================================
        // DÀNH CHO NHÂN VIÊN (Client JS / Calendar)
        // ==========================================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateLeaveRequestModel model, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            await LogActionAsync($"Đăng ký nghỉ: {model.StartDate:dd/MM} - {model.EndDate:dd/MM}");

            var result = await _leaveService.CreateLeaveAsync(model, UserInfo, ct);
            return HandleResult(result);
        }
        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id, [FromBody] CancelBody body, CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            var result = await _leaveService.CancelAsync(id, body.Reason, UserInfo, ct);
            return HandleResult(result);
        }
        [HttpPost("Add")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Add([FromForm] LeaveFormWrapper request)
        {
            if (request?.LeaveForm == null)
                return HandleResult(ServiceResult.Fail("Dữ liệu đơn trống."));

            // Sử dụng AutoMapper để chuyển LeaveFormViewModel -> CreateLeaveRequestModel
            var serviceRequest = _mapper.Map<CreateLeaveRequestModel>(request.LeaveForm);

            await LogActionAsync($"Đăng ký nghỉ: {request.LeaveForm.StartDate:dd/MM} - {request.LeaveForm.EndDate:dd/MM}");

            var result = await _leaveService.CreateLeaveAsync(serviceRequest, UserInfo);
            return HandleResult(result);
        }

        [HttpGet("balance/{year}")]
        public async Task<ActionResult> GetBalance(int year)
        {
            // Lấy EmployeeCode trực tiếp từ Base Class UserInfo
            var result = await _leaveService.GetLeaveBalanceAsync(UserInfo?.EmployeeCode, year);
            return HandleResult(result);
        }

        // ==========================================
        // DÀNH CHO QUẢN LÝ (Approval)
        // ==========================================
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending(CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

            var result = await _leaveQueryService.GetPendingDetailsAsync(
                UserInfo.EmployeeCode, ct);

            await LogActionAsync("Xem danh sách chờ duyệt");
            return Ok(ApiResponse<List<PendingApprovalGroup>>.Ok(result));
        }

        [HttpPost("approve")]
        public async Task<ActionResult> Approve(
    [FromBody] ApproveRequest request,
    CancellationToken ct)           // ✅ THÊM ct
        {
            if (UserInfo == null)           // ✅ THÊM null check
                return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            await LogActionAsync($"Duyệt đơn ID: {string.Join(",", request.Ids)}");
            var result = await _leaveService.ApproveAsync(
                request.Ids, request.Level, UserInfo, request.Comment ?? "", ct);
            return HandleResult(result);
        }

        [HttpPost("reject")]
        public async Task<ActionResult> Reject(
            [FromBody] ApproveRequest request,
            CancellationToken ct)           // ✅ THÊM ct
        {
            if (UserInfo == null)           // ✅ THÊM null check
                return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            await LogActionAsync($"Từ chối đơn ID: {string.Join(",", request.Ids)}");
            var result = await _leaveService.RejectAsync(
                request.Ids, request.Level, UserInfo, request.Comment ?? "", ct);
            return HandleResult(result);
        }
        // Lấy chi tiết đơn để hiển thị trên Modal Timeline
        [HttpGet("details/{id}")]
        public async Task<ActionResult> GetDetails(int id)
        {
            var result = await _leaveService.GetDetailsAsync(id);
            return HandleResult(result);
        }
        [HttpPost("cancel-detail/{detailId}")]
        public async Task<IActionResult> CancelDetail(
    int detailId,
    [FromBody] CancelDetailRequest request,
    CancellationToken ct)
        {
            if (UserInfo == null)
                return Unauthorized(ApiResponse<object>.Fail("Phiên hết hạn"));

            var result = await _leaveService.CancelDetailAsync(
                detailId, request.Reason, UserInfo, ct);

            return HandleResult(result);
        }
    }
    public record CancelBody(string Reason);
    public class LeaveFormWrapper
    {
        // Tên thuộc tính này phải khớp với prefix "LeaveForm" trong name của input HTML
        public LeaveFormViewModel LeaveForm { get; set; }
    }
    public class CancelDetailRequest
    {
        public string Reason { get; set; } = "";
    }
}
