using FVN_REGISTER.Application.Interfaces.PublicInformation;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.PublicInformation;
using FVN_REGISTER.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;

namespace FVN_REGISTER.API.Controllers;

[ApiController]
[Route("api/public-information")]
public sealed class PublicInformationController : ControllerBase
{
    private readonly IPublicInformationService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationService _authorization;

    public PublicInformationController(IPublicInformationService service, ICurrentUserService currentUser, IAuthorizationService authorization)
    { _service = service; _currentUser = currentUser; _authorization = authorization; }

    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Get(CancellationToken ct)
        => Ok(await _service.GetPublishedAsync(ct));

    [Authorize]
    [HttpGet("manage")]
    public async Task<IActionResult> Manage(CancellationToken ct)
    {
        if (!await CanManageAsync(ct)) return Forbid();
        return Ok(await _service.GetManageListAsync(ct));
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOne(int id, CancellationToken ct)
    {
        if (!await CanManageAsync(ct)) return Forbid();
        var item = await _service.GetAsync(id, ct);
        return item == null ? NotFound() : Ok(item);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SavePublicInformationRequest request, CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null) return Unauthorized();
        if (!await CanManageAsync(ct)) return Forbid();
        return Ok(await _service.CreateAsync(request, user.UserId, ct));
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SavePublicInformationRequest request, CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null) return Unauthorized();
        if (!await CanManageAsync(ct)) return Forbid();
        return Ok(await _service.UpdateAsync(id, request, user.UserId, ct));
    }

    [Authorize]
    [HttpPost("{id:int}/publish")]
    public async Task<IActionResult> Publish(int id, CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null) return Unauthorized();
        if (!await CanManageAsync(ct)) return Forbid();
        return Ok(await _service.PublishAsync(id, user.UserId, ct));
    }

    [Authorize]
    [HttpPost("{id:int}/archive")]
    public async Task<IActionResult> Archive(int id, CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null) return Unauthorized();
        if (!await CanManageAsync(ct)) return Forbid();
        return Ok(await _service.ArchiveAsync(id, user.UserId, ct));
    }

    private async Task<bool> CanManageAsync(CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        return user != null && await _authorization.HasAsync(user, SecurityFunctionCodes.PublicInformationManage, ct);
    }
}
