using AutoMapper;
using FVN_REGISTER.Contract.Interfaces.OT;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Contract.ViewModels.OT;
using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOTRequestModel model,
        CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();

        // Gán DeptCode từ user nếu chưa điền
        if (string.IsNullOrEmpty(model.DeptCode))
            model.DeptCode = UserInfo.DeptCode ?? "";

        await LogActionAsync($"Tạo đơn OT ngày {model.OTDate:dd/MM/yyyy}");
        var result = await _otService.CreateAsync(model, UserInfo, ct);
        return HandleResult(result);
    }

    /// <summary>Kiểm tra rule trước khi submit (preview warning)</summary>
    [HttpPost("check-hours")]
    public async Task<IActionResult> CheckHours(
        [FromBody] CheckOTHoursRequest req,
        CancellationToken ct)
    {
        var result = await _otService.CheckOTHoursRuleAsync(
            req.OTDate, req.Employees, ct);
        return Ok(result);
    }

    [HttpPost("approve")]
    public async Task<IActionResult> Approve(
        [FromBody] ApproveRequest request,
        CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        await LogActionAsync($"Duyệt OT: {string.Join(",", request.Ids)}");
        var result = await _otService.ApproveAsync(
            request.Ids, request.Level, UserInfo, request.Comment, ct);
        return HandleResult(result);
    }

    [HttpPost("reject")]
    public async Task<IActionResult> Reject(
        [FromBody] ApproveRequest request,
        CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        var result = await _otService.RejectAsync(
            request.Ids, request.Level, UserInfo, request.Comment, ct);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        var result = await _otService.CancelAsync(id, UserInfo, ct);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _otService.GetByIdAsync(id, ct);
        return HandleResult(result);
    }

    [HttpGet("my/{year?}")]
    public async Task<IActionResult> GetMy(int? year, CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        var result = await _otService.GetByEmployeeAsync(
            UserInfo.EmployeeCode!, year, ct);
        return HandleResult(result);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        var empEmail = await GetApproverEmailAsync(UserInfo.EmployeeCode!, ct);
        var result = await _otService.GetPendingForApproverAsync(empEmail, ct);
        return HandleResult(result);
    }

    private async Task<string> GetApproverEmailAsync(
        string empCode, CancellationToken ct)
    {
        // Tái sử dụng logic lấy email từ bảng approver (giống Leave module)
        using var scope = HttpContext.RequestServices.CreateScope();
        var db = scope.ServiceProvider
            .GetRequiredService<FVN_REGISTER.Contract.Models.FVNWEBAPPContext>();
        return await db.F03leaveDaysApprovers
            .Where(x => x.ApproveLevelCode == empCode)
            .Select(x => x.ApproveLevelEmail)
            .FirstOrDefaultAsync(ct) ?? "";
    }
    /// <summary>Lấy danh sách nhân viên active theo DeptCode</summary>
    [HttpGet("employees/{deptCode}")]
    public async Task<IActionResult> GetEmployeesByDept(
        string deptCode, CancellationToken ct)
    {
        var list = await _db.F03employees
            .AsNoTracking()
            .Where(e => e.DeptCode == deptCode && e.IsActive == true)
            .OrderBy(e => e.EmployeeName)
            .Select(e => new EmployeeSelectDto
            {
                EmployeeCode = e.EmployeeCode,
                EmployeeName = e.EmployeeName,
                DeptCode = e.DeptCode
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<List<EmployeeSelectDto>>.Ok(list));
    }
}

public class CheckOTHoursRequest
{
    public DateOnly OTDate { get; set; }
    public List<OTEmployeeModel> Employees { get; set; } = new();
}