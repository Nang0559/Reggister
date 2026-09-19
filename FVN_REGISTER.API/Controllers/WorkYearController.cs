using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;

namespace FVN_REGISTER.API.Controllers;

[ApiController,Route("api/work-years"),Authorize]
public sealed class WorkYearController : ControllerBase
{
    private readonly IWorkYearManagementService _service; private readonly ICurrentUserService _currentUser; private readonly IAuthorizationService _authorization;
    public WorkYearController(IWorkYearManagementService service,ICurrentUserService currentUser,IAuthorizationService authorization){_service=service;_currentUser=currentUser;_authorization=authorization;}
    [HttpGet] public async Task<ActionResult<List<WorkYearDto>>> GetAll(CancellationToken ct){if(!await CanAsync(ct))return Forbid();return Ok(await _service.GetAllAsync(ct));}
    [HttpPost] public async Task<ActionResult<WorkYearDto>> Create([FromBody]WorkYearDto model,CancellationToken ct){if(!await CanAsync(ct))return Forbid();var u=_currentUser.GetCurrentUser();if(u==null)return Unauthorized();var r=await _service.CreateAsync(model,u.UserId,ct);return r.IsSuccess?Ok(r.Data):BadRequest(r.Message);}
    [HttpPut("{id:int}")] public async Task<ActionResult<WorkYearDto>> Update(int id,[FromBody]WorkYearDto model,CancellationToken ct){if(!await CanAsync(ct))return Forbid();var u=_currentUser.GetCurrentUser();if(u==null)return Unauthorized();model.Id=id;var r=await _service.UpdateAsync(model,u.UserId,ct);return r.IsSuccess?Ok(r.Data):BadRequest(r.Message);}
    [HttpPost("{id:int}/active")] public async Task<IActionResult> SetActive(int id,[FromQuery]bool active,CancellationToken ct){if(!await CanAsync(ct))return Forbid();var u=_currentUser.GetCurrentUser();if(u==null)return Unauthorized();var r=await _service.SetActiveAsync(id,active,u.UserId,ct);return r.IsSuccess?Ok(r.Message):BadRequest(r.Message);}
    private async Task<bool> CanAsync(CancellationToken ct){var u=_currentUser.GetCurrentUser();return u!=null&&await _authorization.HasAsync(u,SecurityFunctionCodes.SecurityView,ct);}
}