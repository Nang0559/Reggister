using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.UserManagers;
using FVN_REGISTER.Contract.Dtos.Usermanagers;
using FVN_REGISTER.Contract.Requests.Users;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Entities.Security;
using FVN_REGISTER.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FVN_REGISTER.API.Controllers;

[ApiController]
[Route("api/password-reset-requests")]
public sealed class PasswordResetRequestsController : BaseApiController
{
    private readonly IUnitOfWork _uow;
    private readonly IUserManagementService _userManagement;
    private readonly AppAuthorizationService _authorization;

    public PasswordResetRequestsController(
        IUnitOfWork uow,
        IUserManagementService userManagement,
        ICurrentUserService currentUser,
        IUserLogService userLog,
        ILogger<PasswordResetRequestsController> logger,
        Microsoft.Extensions.Options.IOptionsMonitor<FVN_REGISTER.Application.Configuration.AuthDebugOptions> options,
        AppAuthorizationService authorization)
        : base(currentUser, userLog, logger, options)
    {
        _uow = uow;
        _userManagement = userManagement;
        _authorization = authorization;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Submit(
        [FromBody] PasswordResetRequestCreateDto request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ."));

        var employeeCode = request.EmployeeCode.Trim();
        if (employeeCode.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("Mã nhân viên không được để trống."));

        var employee = await _uow.Repository<F03Employee>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeCode == employeeCode, ct);

        var user = await _uow.Repository<F03User>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeCode == employeeCode, ct);

        // Do not reveal whether the account exists. The login screen always shows
        // the same success message for a syntactically valid request.
        if (employee != null && user != null && (user.IsActive ?? false))
        {
            var pendingExists = await _uow.Repository<F03PasswordResetRequest>().Query()
                .AnyAsync(x => x.EmployeeCode == employeeCode && x.Status == "Pending", ct);

            if (!pendingExists)
            {
                await _uow.Repository<F03PasswordResetRequest>().AddAsync(
                    new F03PasswordResetRequest
                    {
                        EmployeeCode = employeeCode,
                        FullName = employee.EmployeeName,
                        DeptCode = employee.DeptCode,
                        RequestNote = request.RequestNote?.Trim(),
                        Status = "Pending",
                        RequestedAt = DateTime.Now,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = Request.Headers.UserAgent.ToString()
                    }, ct);

                await _uow.SaveChangesAsync(ct);
            }
        }

        return Ok(ApiResponse<object>.Ok(new
        {
            Message = "Yêu cầu đã được ghi nhận và chuyển đến IT."
        }));
    }

    [Authorize]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.UserManagementResetPassword, ct))
            return Forbid();

        var rows = await _uow.Repository<F03PasswordResetRequest>().Query()
            .AsNoTracking()
            .Where(x => x.Status == "Pending")
            .OrderBy(x => x.RequestedAt)
            .Select(x => new PasswordResetRequestDto
            {
                Id = x.Id,
                EmployeeCode = x.EmployeeCode,
                FullName = x.FullName,
                DeptCode = x.DeptCode,
                RequestNote = x.RequestNote,
                Status = x.Status,
                RequestedAt = x.RequestedAt,
                ProcessedAt = x.ProcessedAt,
                ProcessedBy = x.ProcessedBy,
                ProcessorName = x.ProcessorName,
                ResultNote = x.ResultNote
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<List<PasswordResetRequestDto>>.Ok(rows));
    }

    [Authorize]
    [HttpPut("{id:int}/process")]
    public async Task<IActionResult> Process(
        int id,
        [FromBody] PasswordResetRequestProcessDto request,
        CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.UserManagementResetPassword, ct))
            return Forbid();

        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ."));

        var operatorUser = UserInfo;
        if (operatorUser == null)
            return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không hợp lệ hoặc đã hết hạn."));

        var row = await _uow.Repository<F03PasswordResetRequest>().Query()
            .FirstOrDefaultAsync(x => x.Id == id && x.Status == "Pending", ct);

        if (row == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy yêu cầu đang chờ xử lý."));

        var target = await _uow.Repository<F03User>().Query()
            .FirstOrDefaultAsync(x => x.EmployeeCode == row.EmployeeCode, ct);

        if (target == null || !(target.IsActive ?? false))
            return BadRequest(ApiResponse<object>.Fail("Tài khoản nhân viên không còn hoạt động."));

        var employee = await _uow.Repository<F03Employee>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeCode == row.EmployeeCode, ct);

        if (!request.Approve)
        {
            row.Status = "Rejected";
            row.ProcessedAt = DateTime.Now;
            row.ProcessedBy = operatorUser.UserId;
            row.ProcessorName = operatorUser.UserName;
            row.ResultNote = request.ResultNote?.Trim() ?? "IT từ chối yêu cầu.";
            await _uow.SaveChangesAsync(ct);

            await WriteAuditAsync(
                operatorUser.UserId,
                operatorUser.UserName,
                "PASSWORD_RESET_REQUEST_REJECTED",
                $"EmployeeCode={row.EmployeeCode}; RequestId={row.Id}; Reason={row.ResultNote}",
                ct);

            return Ok(ApiResponse<PasswordResetRequestDto>.Ok(Map(row)));
        }

        var reset = await _userManagement.ResetPasswordAsync(
            target.Id, "FVN@123", operatorUser.UserId, ct);

        if (!reset.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(reset.Message ?? "Không thể reset mật khẩu."));

        row.Status = "Completed";
        row.ProcessedAt = DateTime.Now;
        row.ProcessedBy = operatorUser.UserId;
        row.ProcessorName = operatorUser.UserName;
        row.ResultNote = request.ResultNote?.Trim() ?? "Đã reset mật khẩu tạm thời về FVN@123.";
        await _uow.SaveChangesAsync(ct);

        await WriteAuditAsync(
            operatorUser.UserId,
            operatorUser.UserName,
            "PASSWORD_RESET_REQUEST_COMPLETED",
            $"EmployeeCode={row.EmployeeCode}; RequestId={row.Id}; TemporaryPassword=FVN@123",
            ct);

        return Ok(ApiResponse<PasswordResetRequestDto>.Ok(Map(row)));
    }

    private async Task WriteAuditAsync(
        int userId,
        string? userName,
        string action,
        string description,
        CancellationToken ct)
    {
        await _uow.Repository<F03AuditLog>().AddAsync(
            new F03AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = action,
                Description = description,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString()
            }, ct);

        await _uow.SaveChangesAsync(ct);
    }

    private static PasswordResetRequestDto Map(F03PasswordResetRequest x)
        => new()
        {
            Id = x.Id,
            EmployeeCode = x.EmployeeCode,
            FullName = x.FullName,
            DeptCode = x.DeptCode,
            RequestNote = x.RequestNote,
            Status = x.Status,
            RequestedAt = x.RequestedAt,
            ProcessedAt = x.ProcessedAt,
            ProcessedBy = x.ProcessedBy,
            ProcessorName = x.ProcessorName,
            ResultNote = x.ResultNote
        };

    private async Task<bool> CanAsync(int functionCode, CancellationToken ct)
        => UserInfo != null &&
           await _authorization.HasAsync(UserInfo, functionCode, ct);
}
