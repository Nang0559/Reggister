using AutoMapper;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Interfaces.OT;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.ViewModels.OT;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OTController : BaseApiController
{
    private readonly IOTService _otService;
    private readonly IOTQueryService _queryService;

    public OTController(
        IOTService otService,
        IOTQueryService queryService,
        ICurrentUserService currentUser,
        IUserLogService userLog,
        IMapper mapper,
        ILogger<OTController> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(currentUser, userLog, mapper, logger, options)
    {
        _otService = otService;
        _queryService = queryService;
    }

    // ===== TẠO ĐƠN OT =====
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOTRequestModel model, CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        var result = await _otService.CreateAsync(model, UserInfo, ct);
        await LogActionAsync($"Tạo đơn OT ngày {model.OTDate:dd/MM/yyyy}");
        return HandleResult(result);
    }

    // ===== DUYỆT ĐƠN =====
    [HttpPost("approve")]
    public async Task<IActionResult> Approve(
        [FromBody] OTApproveRequest request, CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        var result = await _otService.ApproveAsync(
            request.Ids, request.Level, UserInfo, request.Comment, ct);
        await LogActionAsync($"Duyệt OT Level {request.Level}");
        return HandleResult(result);
    }

    // ===== TỪ CHỐI =====
    [HttpPost("reject")]
    public async Task<IActionResult> Reject(
        [FromBody] OTApproveRequest request, CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.Comment))
            return BadRequest(ApiResponse<object>.Fail("Phải nhập lý do từ chối."));
        var result = await _otService.RejectAsync(
            request.Ids, request.Level, UserInfo, request.Comment!, ct);
        await LogActionAsync($"Từ chối OT Level {request.Level}");
        return HandleResult(result);
    }

    // ===== HỦY ĐƠN =====
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(
        int id, [FromBody] string reason, CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        var result = await _otService.CancelAsync(id, reason, UserInfo, ct);
        return HandleResult(result);
    }

    // ===== XÁC NHẬN GIỜ THỰC TẾ =====
    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> ConfirmActual(
        int id,
        [FromBody] ConfirmOTRequest req,
        CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        var result = await _otService.ConfirmActualHoursAsync(
            id, req.ActualFrom, req.ActualTo, UserInfo, ct);
        return HandleResult(result);
    }

    // ===== CHI TIẾT =====
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetails(int id, CancellationToken ct)
    {
        var result = await _otService.GetDetailsAsync(id, ct);
        return HandleResult(result);
    }

    // ===== DANH SÁCH CHỜ DUYỆT CỦA TÔI =====
    [HttpGet("pending")]
    public async Task<IActionResult> GetMyPending(CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        var result = await _queryService.GetPendingForApproverAsync(
            UserInfo.Email ?? "", ct);
        return HandleResult(result);
    }

    // ===== LỊCH SỬ OT CỦA NHÂN VIÊN =====
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int? year, CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        var result = await _queryService.GetByEmployeeAsync(
            UserInfo.EmployeeCode!, year ?? DateTime.Now.Year, ct);
        return HandleResult(result);
    }

    // ===== DANH SÁCH NGƯỜI KÝ (dropdown) =====
    [HttpGet("approvers")]
    public async Task<IActionResult> GetApprovers(
        [FromQuery] string deptCode,
        [FromQuery] int level,
        CancellationToken ct)
    {
        var result = await _queryService.GetApproversAsync(deptCode, level, ct);
        return HandleResult(result);
    }

    // ===== TỔNG GIỜ OT =====
    [HttpGet("summary/{employeeCode}")]
    public async Task<IActionResult> GetSummary(
        string employeeCode,
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken ct)
    {
        var result = await _queryService.GetSummaryAsync(employeeCode, year, month, ct);
        return HandleResult(result);
    }
}