using FVN_REGISTER.Application.Interfaces.Companies;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FVN_REGISTER.API.Controllers;
[ApiController,Route("api/company-holidays"),Authorize]
public sealed class CompanyHolidayManagementController:ControllerBase
{
 readonly ICompanyHolidayManagementService _service;readonly ICurrentUserService _currentUser;readonly IAuthorizationService _authorization;
 public CompanyHolidayManagementController(ICompanyHolidayManagementService service,ICurrentUserService currentUser,IAuthorizationService authorization){_service=service;_currentUser=currentUser;_authorization=authorization;}
 [HttpGet]public async Task<ActionResult<List<CompanyHolidayDto>>> GetAll(CancellationToken ct){if(!await Can(ct))return Forbid();return Ok(await _service.GetAllAsync(ct));}
 [HttpGet("years")]public async Task<ActionResult<List<int>>> Years(CancellationToken ct){if(!await Can(ct))return Forbid();return Ok(await _service.GetWorkYearsAsync(ct));}
 [HttpPost]public async Task<ActionResult<CompanyHolidayDto>> Create([FromBody]CompanyHolidayDto m,CancellationToken ct){if(!await Can(ct))return Forbid();var u=_currentUser.GetCurrentUser();if(u==null)return Unauthorized();var r=await _service.CreateAsync(m,u.UserId,ct);return r.IsSuccess?Ok(r.Data):BadRequest(r.Message);}
 [HttpPut("{id:int}")]public async Task<ActionResult<CompanyHolidayDto>> Update(int id,[FromBody]CompanyHolidayDto m,CancellationToken ct){if(!await Can(ct))return Forbid();var u=_currentUser.GetCurrentUser();if(u==null)return Unauthorized();m.Id=id;var r=await _service.UpdateAsync(m,u.UserId,ct);return r.IsSuccess?Ok(r.Data):BadRequest(r.Message);}
 [HttpDelete("{id:int}")]public async Task<IActionResult> Delete(int id,CancellationToken ct){if(!await Can(ct))return Forbid();var r=await _service.DeleteAsync(id,ct);return r.IsSuccess?Ok(r.Message):BadRequest(r.Message);}
 [HttpPost("sundays/{year:int}")]public async Task<IActionResult>Sundays(int year,CancellationToken ct){if(!await Can(ct))return Forbid();var u=_currentUser.GetCurrentUser();if(u==null)return Unauthorized();var r=await _service.CreateSundaysAsync(year,u.UserId,ct);return r.IsSuccess?Ok(r.Message):BadRequest(r.Message);}
 [HttpPost("import")] [RequestSizeLimit(10_000_000)] public async Task<IActionResult> Import(IFormFile file,CancellationToken ct){if(!await Can(ct))return Forbid();if(file==null||file.Length==0)return BadRequest("File Excel rỗng.");var u=_currentUser.GetCurrentUser();if(u==null)return Unauthorized();await using var s=file.OpenReadStream();var r=await _service.ImportExcelAsync(s,file.FileName,u.UserId,ct);return r.IsSuccess?Ok(r.Message):BadRequest(r.Message);}
 async Task<bool> Can(CancellationToken ct){var u=_currentUser.GetCurrentUser();return u!=null&&await _authorization.HasAsync(u,SecurityFunctionCodes.SecurityView,ct);}
}