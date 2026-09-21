using FVN_REGISTER.Application.Interfaces.PublicForms;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FVN_REGISTER.API.Controllers;

[ApiController]
[Route("api/public-forms")]
[Authorize]
public sealed class PublicFormsController : ControllerBase
{
    private readonly IPublicFormService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationService _authorization;
    public PublicFormsController(IPublicFormService service, ICurrentUserService currentUser, IAuthorizationService authorization){_service=service;_currentUser=currentUser;_authorization=authorization;}

    [HttpGet("manage")]
    public async Task<IActionResult> Manage(CancellationToken ct){if(!await CanManageAsync(ct))return Forbid();return Ok(await _service.GetManageListAsync(ct));}

    [HttpGet("available")]
    public async Task<IActionResult> Available(CancellationToken ct)
    {
        var user=_currentUser.GetCurrentUser();
        if(user==null)return Unauthorized();
        return Ok(await _service.GetAvailableAsync(user.EmployeeCode??string.Empty,user.DeptCode,user.PositionCode,ct));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id,CancellationToken ct){var x=await _service.GetAsync(id,ct);return x==null?NotFound():Ok(x);}

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Contract.Requests.PublicForms.SavePublicFormRequest request,CancellationToken ct)
    {
        var u=_currentUser.GetCurrentUser(); if(u==null)return Unauthorized(); if(!await CanManageAsync(ct))return Forbid();
        return Ok(await _service.CreateAsync(request,u.UserId,ct));
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id,[FromBody] Contract.Requests.PublicForms.SavePublicFormRequest request,CancellationToken ct)
    {
        var u=_currentUser.GetCurrentUser(); if(u==null)return Unauthorized(); if(!await CanManageAsync(ct))return Forbid();
        return Ok(await _service.UpdateAsync(id,request,u.UserId,ct));
    }
    [HttpPost("{id:int}/publish")]
    public async Task<IActionResult> Publish(int id,CancellationToken ct){var u=_currentUser.GetCurrentUser();if(u==null)return Unauthorized();if(!await CanManageAsync(ct))return Forbid();return Ok(await _service.PublishAsync(id,u.UserId,ct));}
    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> Submit(int id,[FromBody] List<Contract.Requests.PublicForms.PublicFormAnswerRequest> answers,CancellationToken ct)
    {
        var u=_currentUser.GetCurrentUser(); if(u==null)return Unauthorized();
        return Ok(await _service.SubmitAsync(id,u.EmployeeCode??string.Empty,u.DeptCode,u.PositionCode,answers,ct));
    }

    [HttpPost("{id:int}/close")]
    public async Task<IActionResult> Close(int id,CancellationToken ct){var u=_currentUser.GetCurrentUser();if(u==null)return Unauthorized();if(!await CanManageAsync(ct))return Forbid();return Ok(await _service.CloseAsync(id,u.UserId,ct));}

    private async Task<bool> CanManageAsync(CancellationToken ct)
    {
        var user=_currentUser.GetCurrentUser();
        return user!=null && await _authorization.HasAsync(user,SecurityFunctionCodes.PublicFormManage,ct);
    }
}