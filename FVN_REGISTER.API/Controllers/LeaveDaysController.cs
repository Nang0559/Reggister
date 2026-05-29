using AutoMapper;
using FVN_REGISTER.Contract.Interfaces.Leaves;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        }

        // ==========================================
        // DÀNH CHO NHÂN VIÊN (Client JS / Calendar)
        // ==========================================

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

        [HttpPost("approve")]
        public async Task<ActionResult> Approve([FromBody] ApproveRequest request)
        {
            await LogActionAsync($"Duyệt đơn ID: {string.Join(",", request.Ids)}");
            var result = await _leaveService.ApproveAsync(request.Ids, request.Level, UserInfo, request.Comment);
            return HandleResult(result);
        }

        [HttpPost("reject")]
        public async Task<ActionResult> Reject([FromBody] ApproveRequest request)
        {
            await LogActionAsync($"Từ chối đơn ID: {string.Join(",", request.Ids)}");
            var result = await _leaveService.RejectAsync(request.Ids, request.Level, UserInfo, request.Comment);
            return HandleResult(result);
        }

        // Lấy chi tiết đơn để hiển thị trên Modal Timeline
        [HttpGet("details/{id}")]
        public async Task<ActionResult> GetDetails(int id)
        {
            var result = await _leaveService.GetDetailsAsync(id);
            return HandleResult(result);
        }
    }
    public class LeaveFormWrapper
    {
        // Tên thuộc tính này phải khớp với prefix "LeaveForm" trong name của input HTML
        public LeaveFormViewModel LeaveForm { get; set; }
    }
}
