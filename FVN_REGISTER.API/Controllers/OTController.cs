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

    public OTController(
        IOTService otService,
        ICurrentUserService currentUser,
        IUserLogService userLog,
        IMapper mapper,
        ILogger<OTController> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(currentUser, userLog, mapper, logger, options)
    {
        _otService = otService;
    }

    // ── GET DETAILS ─────────────────────────────────────────────
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDetails(int id, CancellationToken ct)
    {
        var result = await _otService.GetDetailsAsync(id, ct);
        return HandleResult(result);
    }

    // ── GET BALANCE (giờ OT còn lại của nhân viên) ────────────────
    [HttpGet("balance/{employeeCode}/{year:int}/{month:int}")]
    public async Task<IActionResult> GetBalance(
        string employeeCode, int year, int month, CancellationToken ct)
    {
        var result = await _otService.GetOTBalanceAsync(employeeCode, year, month, ct);
        return HandleResult(result);
    }

    // ── TẠO ĐƠN OT ─────────────────────────────────────────────
    /// <summary>
    /// Bước 1-2: Tạo đơn OT, chọn phạm vi nhân viên và người duyệt.
    /// CVCode 0003 (công nhân) cần thêm Level1 (Sub-leader).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOTRequestModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ."));

        if (UserInfo == null)
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

        await LogActionAsync($"Tạo đơn OT ngày {model.OTDate:dd/MM/yyyy}");
        var result = await _otService.CreateOTAsync(model, UserInfo, ct);
        return HandleResult(result);
    }

    // ── DUYỆT ─────────────────────────────────────────────────
    /// <summary>
    /// Level 1 = Sub-leader/Leader (Bước 3 - chỉ công nhân)
    /// Level 2 = Ast.Chief/Chief  (Bước 5)
    /// Level 3 = A.MG/MG         (Bước 6)
    /// Level 4 = GM               (Bước 7)
    /// </summary>
    [HttpPost("approve")]
    public async Task<IActionResult> Approve(
        [FromBody] OTApproveRequest request, CancellationToken ct)
    {
        if (UserInfo == null)
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

        await LogActionAsync($"Duyệt OT Level {request.Level}: {string.Join(",", request.Ids)}");
        var result = await _otService.ApproveAsync(request.Ids, request.Level, UserInfo, request.Comment, ct);
        return HandleResult(result);
    }

    // ── TỪ CHỐI ─────────────────────────────────────────────────
    [HttpPost("reject")]
    public async Task<IActionResult> Reject(
        [FromBody] OTApproveRequest request, CancellationToken ct)
    {
        if (UserInfo == null)
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

        await LogActionAsync($"Từ chối OT Level {request.Level}: {string.Join(",", request.Ids)}");
        var result = await _otService.RejectAsync(request.Ids, request.Level, UserInfo, request.Comment, ct);
        return HandleResult(result);
    }

    // ── HỦY ─────────────────────────────────────────────────────
    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromBody] string? reason, CancellationToken ct)
    {
        if (UserInfo == null)
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

        await LogActionAsync($"Hủy đơn OT ID: {id}");
        var result = await _otService.CancelAsync(id, reason, UserInfo, ct);
        return HandleResult(result);
    }

    // ── VALIDATE & ARCHIVE (GA lưu trữ - Bước 9-10) ────────────
    [HttpPost("{id:int}/archive")]
    public async Task<IActionResult> Archive(
        int id, [FromBody] string? note, CancellationToken ct)
    {
        if (UserInfo == null)
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập hết hạn."));

        await LogActionAsync($"Lưu trữ đơn OT ID: {id}");
        var result = await _otService.ValidateAndArchiveAsync(id, UserInfo, note, ct);
        return HandleResult(result);
    }
}