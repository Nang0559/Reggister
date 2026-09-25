using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.UserManagers;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Contract.Dtos.Usermanagers;
using FVN_REGISTER.Contract.Requests.Users;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Core.Entities.HR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AppAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;

namespace FVN_REGISTER.API.Controllers;

[Authorize]
[ApiController]
[Route("api/user-management")]
public class UserManagementController : BaseApiController
{
    private readonly IUserManagementService _userMgt;
    private readonly AppAuthorizationService _authorization;
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _emailService;

    public UserManagementController(IUserManagementService userMgt, ICurrentUserService currentUser, IUserLogService userLog, ILogger<UserManagementController> logger, IOptionsMonitor<AuthDebugOptions> options, AppAuthorizationService authorization, IUnitOfWork uow, IEmailService emailService)
        : base(currentUser, userLog, logger, options)
    { _userMgt = userMgt; _authorization = authorization; _uow = uow; _emailService = emailService; }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    { if (!await CanAsync(SecurityFunctionCodes.UserManagementView, ct)) return Forbid(); return Ok(ApiResponse<List<UserAccountDto>>.Ok(await _userMgt.GetAllAsync(ct))); }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    { if (!await CanAsync(SecurityFunctionCodes.UserManagementView, ct)) return Forbid(); var result = await _userMgt.GetByIdAsync(id, ct); if (result == null) return NotFound(ApiResponse<object>.Fail("Không tìm thấy.")); return Ok(ApiResponse<UserAccountDto>.Ok(result)); }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.UserManagementCreate, ct)) return Forbid();
        var user = UserInfo;
        if (user == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không hợp lệ hoặc đã hết hạn."));

        var result = await _userMgt.CreateAsync(request, user.UserId, ct);
        if (!result.Success) return HandleResult(result);

        var employee = await _uow.Repository<F03Employee>().Query()
            .Where(x => x.EmployeeCode == request.EmployeeCode)
            .Select(x => new { x.EmployeeCode, x.EmployeeName, x.EmailAddress, x.DeptCode })
            .FirstOrDefaultAsync(ct);

        if (employee != null && !string.IsNullOrWhiteSpace(employee.EmailAddress))
        {
            var emailResult = await _emailService.QueueEmail(employee.EmailAddress, "USER_ACCOUNT_CREATED", new
            {
                employee.EmployeeCode,
                EmployeeName = employee.EmployeeName,
                UserName = request.EmployeeCode,
                DepartmentCode = employee.DeptCode
            }, ct);
            if (!emailResult.IsSuccess) _logger.LogWarning("[USER_MGT] Account created but welcome email was not queued for {EmployeeCode}: {Message}", request.EmployeeCode, emailResult.Message);
        }
        else Logger.LogWarning("[USER_MGT] Account created but employee email is missing for {EmployeeCode}", request.EmployeeCode);

        return HandleResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateUserRequest request, CancellationToken ct)
    { if (!await CanAsync(SecurityFunctionCodes.UserManagementEdit, ct)) return Forbid(); var user = UserInfo; if (user == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không hợp lệ hoặc đã hết hạn.")); return HandleResult(await _userMgt.UpdateAsync(request, user.UserId, ct)); }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    { if (!await CanAsync(SecurityFunctionCodes.UserManagementEdit, ct)) return Forbid(); var user = UserInfo; if (user == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không hợp lệ hoặc đã hết hạn.")); if (id == user.UserId) return BadRequest(ApiResponse<object>.Fail("Không thể xóa tài khoản đang đăng nhập.")); return HandleResult(await _userMgt.DeleteAsync(id, user.UserId, ct)); }

    [HttpPut("{id:int}/toggle-lock")]
    public async Task<IActionResult> ToggleLock(int id, CancellationToken ct)
    { if (!await CanAsync(SecurityFunctionCodes.UserManagementLock, ct)) return Forbid(); var user = UserInfo; if (user == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không hợp lệ hoặc đã hết hạn.")); if (id == user.UserId) return BadRequest(ApiResponse<object>.Fail("Không thể khóa tài khoản đang đăng nhập.")); return HandleResult(await _userMgt.ToggleLockAsync(id, user.UserId, ct)); }

    [HttpPut("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequestDto request, CancellationToken ct)
    { if (!await CanAsync(SecurityFunctionCodes.UserManagementResetPassword, ct)) return Forbid(); if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ.")); var user = UserInfo; if (user == null) return Unauthorized(ApiResponse<object>.Fail("Phiên đăng nhập không hợp lệ hoặc đã hết hạn.")); return HandleResult(await _userMgt.ResetPasswordAsync(id, request.NewPassword, user.UserId, ct)); }

    private async Task<bool> CanAsync(int functionCode, CancellationToken ct) => UserInfo != null && await _authorization.HasAsync(UserInfo, functionCode, ct);
}
