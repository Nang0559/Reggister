using FVN_REGISTER.Application.Interfaces.Companies;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FVN_REGISTER.API.Controllers;

[ApiController, Route("api/company-holidays"), Authorize]
public sealed class CompanyHolidayManagementController : ControllerBase
{
    readonly ICompanyHolidayManagementService _service;
    readonly ICurrentUserService _currentUser;
    readonly IAuthorizationService _authorization;

    public CompanyHolidayManagementController(
        ICompanyHolidayManagementService service,
        ICurrentUserService currentUser,
        IAuthorizationService authorization)
    {
        _service = service;
        _currentUser = currentUser;
        _authorization = authorization;
    }

    [HttpGet]
    public async Task<ActionResult<List<CompanyHolidayDto>>> GetAll(CancellationToken ct)
        => await CanView(ct) ? Ok(await _service.GetAllAsync(ct)) : Forbid();

    [HttpGet("years")]
    public async Task<ActionResult<List<int>>> Years(CancellationToken ct)
        => await CanView(ct) ? Ok(await _service.GetWorkYearsAsync(ct)) : Forbid();

    [HttpGet("template")]
    public async Task<IActionResult> Template(CancellationToken ct)
    {
        if (!await CanManage(ct))
            return Forbid();

        var result = await _service.DownloadTemplateAsync(ct);
        return result.IsSuccess && result.Data is { Length: > 0 }
            ? File(
                result.Data,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "FCC_CompanyHoliday_Import_Template.xlsx")
            : BadRequest(result.Message);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyHolidayDto>> Create(
        [FromBody] CompanyHolidayDto model,
        CancellationToken ct)
    {
        if (!await Can(ct)) return Forbid();

        var user = _currentUser.GetCurrentUser();
        if (user == null) return Unauthorized();

        var result = await _service.CreateAsync(model, user.UserId, ct);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Message);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CompanyHolidayDto>> Update(
        int id,
        [FromBody] CompanyHolidayDto model,
        CancellationToken ct)
    {
        if (!await Can(ct)) return Forbid();

        var user = _currentUser.GetCurrentUser();
        if (user == null) return Unauthorized();

        model.Id = id;
        var result = await _service.UpdateAsync(model, user.UserId, ct);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Message);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (!await Can(ct)) return Forbid();

        var result = await _service.DeleteAsync(id, ct);
        return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
    }

    [HttpPost("sundays/{year:int}")]
    public async Task<IActionResult> Sundays(int year, CancellationToken ct)
    {
        if (!await Can(ct)) return Forbid();

        var user = _currentUser.GetCurrentUser();
        if (user == null) return Unauthorized();

        var result = await _service.CreateSundaysAsync(year, user.UserId, ct);
        return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
    }

    [HttpPost("import")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Import(IFormFile file, CancellationToken ct)
    {
        if (!await Can(ct)) return Forbid();
        if (file == null || file.Length == 0)
            return BadRequest("File Excel rỗng.");

        if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Chỉ hỗ trợ file Excel .xlsx.");

        var user = _currentUser.GetCurrentUser();
        if (user == null) return Unauthorized();

        await using var stream = file.OpenReadStream();
        var result = await _service.ImportExcelAsync(stream, file.FileName, user.UserId, ct);
        return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
    }

    async Task<bool> CanView(CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        return user != null &&
               await _authorization.HasAsync(user, SecurityFunctionCodes.WorkCalendarView, ct);
    }

    async Task<bool> CanManage(CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        return user != null && await _authorization.HasAsync(user, SecurityFunctionCodes.WorkCalendarManage, ct);
    }
}