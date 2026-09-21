using FVN_REGISTER.Application.Interfaces.PublicForms;
using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Logging;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using IAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;
using FVN_REGISTER.Contract.Utils;

namespace FVN_REGISTER.API.Controllers;

[ApiController]
[Route("api/public-forms")]
[Authorize]
public sealed class PublicFormsController : BaseApiController
{
    private readonly IPublicFormService _service;
    private readonly IAuthorizationService _authorization;

    public PublicFormsController(
        IPublicFormService service,
        ICurrentUserService currentUser,
        IUserLogService userLog,
        IAuthorizationService authorization,
        ILogger<PublicFormsController> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(currentUser, userLog, logger, options)
    {
        _service = service;
        _authorization = authorization;
    }

    [HttpGet("manage")]
    public async Task<IActionResult> Manage(CancellationToken ct){if(!await CanManageAsync(ct))return Forbid();return HandleResult(ServiceResult<List<Contract.Dtos.PublicForms.PublicFormDto>>.Ok(await _service.GetManageListAsync(ct)));}

    [HttpGet("available")]
    public async Task<IActionResult> Available(CancellationToken ct)
    {
        if(UserInfo==null)return Unauthorized();
        return HandleResult(ServiceResult<List<Contract.Dtos.PublicForms.PublicFormDto>>.Ok(await _service.GetAvailableAsync(UserInfo.EmployeeCode??string.Empty,UserInfo.DeptCode,UserInfo.PositionCode,ct)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id,CancellationToken ct){var x=await _service.GetAsync(id,ct);return x==null?NotFound(ApiResponse<object>.Fail("Không tìm thấy biểu mẫu.")):Ok(ApiResponse<Contract.Dtos.PublicForms.PublicFormDto>.Ok(x));}

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Contract.Requests.PublicForms.SavePublicFormRequest request,CancellationToken ct)
    {
        if(UserInfo==null)return Unauthorized(); if(!await CanManageAsync(ct))return Forbid();
        var result=await _service.CreateAsync(request,UserInfo.UserId,ct); await LogActionAsync($"Tạo biểu mẫu {request.FormCode}"); return HandleResult(result);
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id,[FromBody] Contract.Requests.PublicForms.SavePublicFormRequest request,CancellationToken ct)
    {
        if(UserInfo==null)return Unauthorized(); if(!await CanManageAsync(ct))return Forbid();
        return HandleResult(await _service.UpdateAsync(id,request,UserInfo.UserId,ct));
    }
    [HttpPost("{id:int}/publish")]
    public async Task<IActionResult> Publish(int id,CancellationToken ct){if(UserInfo==null)return Unauthorized();if(!await CanManageAsync(ct))return Forbid();var r=await _service.PublishAsync(id,UserInfo.UserId,ct);if(r.IsSuccess)await LogActionAsync($"Publish biểu mẫu {id}");return HandleResult(r);}
    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> Submit(int id,[FromBody] List<Contract.Requests.PublicForms.PublicFormAnswerRequest> answers,CancellationToken ct)
    {
        if(UserInfo==null)return Unauthorized();
        return HandleResult(await _service.SubmitAsync(id,UserInfo.EmployeeCode??string.Empty,UserInfo.DeptCode,UserInfo.PositionCode,answers,ct));
    }

    [HttpPost("{id:int}/close")]
    public async Task<IActionResult> Close(int id,CancellationToken ct){if(UserInfo==null)return Unauthorized();if(!await CanManageAsync(ct))return Forbid();var r=await _service.CloseAsync(id,UserInfo.UserId,ct);if(r.IsSuccess)await LogActionAsync($"Đóng biểu mẫu {id}");return HandleResult(r);}


    [HttpGet("submissions/forms")]
    public async Task<IActionResult> SubmissionForms(CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.PublicFormSubmissionView, ct))
            return Forbid();

        return HandleResult(ServiceResult<List<Contract.Dtos.PublicForms.PublicFormDto>>.Ok(
            await _service.GetSubmissionFormsAsync(ct)));
    }

    [HttpGet("{id:int}/submissions")]
    public async Task<IActionResult> Submissions(
        int id,
        [FromQuery] Contract.Dtos.PublicForms.PublicFormSubmissionQueryDto query,
        CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.PublicFormSubmissionView, ct))
            return Forbid();

        var scope = await _authorization.GetScopeAsync(
            UserInfo.UserId,
            SecurityFunctionCodes.PublicFormSubmissionView,
            ct);

        if (string.Equals(scope, AuthorizationScopeCodes.Department, StringComparison.OrdinalIgnoreCase))
            query.DepartmentCode = UserInfo.DeptCode;

        return HandleResult(await _service.GetSubmissionsAsync(id, query, scope, ct));
    }

    [HttpGet("{id:int}/submissions/summary")]
    public async Task<IActionResult> SubmissionSummary(
        int id,
        [FromQuery] Contract.Dtos.PublicForms.PublicFormSubmissionQueryDto query,
        CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.PublicFormSubmissionView, ct))
            return Forbid();

        var scope = await _authorization.GetScopeAsync(
            UserInfo.UserId,
            SecurityFunctionCodes.PublicFormSubmissionView,
            ct);

        if (string.Equals(scope, AuthorizationScopeCodes.Department, StringComparison.OrdinalIgnoreCase))
            query.DepartmentCode = UserInfo.DeptCode;

        return HandleResult(await _service.GetSubmissionSummaryAsync(id, query, scope, ct));
    }

    [HttpGet("{id:int}/submissions/export")]
    public async Task<IActionResult> ExportSubmissions(
        int id,
        [FromQuery] Contract.Dtos.PublicForms.PublicFormSubmissionQueryDto query,
        CancellationToken ct)
    {
        if (UserInfo == null) return Unauthorized();
        if (!await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.PublicFormExport, ct))
            return Forbid();

        var scope = await _authorization.GetScopeAsync(
            UserInfo.UserId,
            SecurityFunctionCodes.PublicFormExport,
            ct);

        if (string.Equals(scope, AuthorizationScopeCodes.Department, StringComparison.OrdinalIgnoreCase))
            query.DepartmentCode = UserInfo.DeptCode;

        var result = await _service.ExportSubmissionsAsync(id, query, scope, ct);
        if (!result.IsSuccess || result.Data == null)
            return HandleResult(result);

        var fileName = $"PublicForm_{id}_Submissions_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(result.Data,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    private async Task<bool> CanManageAsync(CancellationToken ct)
    {
        return UserInfo != null && await _authorization.HasAsync(UserInfo, SecurityFunctionCodes.PublicFormManage, ct);
    }
}