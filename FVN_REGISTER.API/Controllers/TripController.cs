using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Trips;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Trips;
using FVN_REGISTER.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FVN_REGISTER.API.Controllers;

[ApiController]
[Route("api/trips")]
[Authorize]
public sealed class TripController : ControllerBase
{
    private readonly ITripService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationService _authorization;

    public TripController(
        ITripService service,
        ICurrentUserService currentUser,
        IAuthorizationService authorization)
    {
        _service = service;
        _currentUser = currentUser;
        _authorization = authorization;
    }

    [HttpPost]
    public async Task<ActionResult<TripRequestDto>> Create(
        [FromBody] CreateTripRequestDto request,
        CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.TripCreate, ct)) return Forbid();
        return Ok(await _service.CreateDraftAsync(request, ct));
    }

    [HttpPost("{id:int}/submit")]
    public async Task<ActionResult<TripRequestDto>> Submit(int id, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.TripEdit, ct)) return Forbid();
        return Ok(await _service.SubmitAsync(id, ct));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TripRequestDto>> Get(int id, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.TripView, ct)) return Forbid();
        var result = await _service.GetAsync(id, ct);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<TripRequestDto>>> Mine(CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.TripView, ct)) return Forbid();
        return Ok(await _service.GetMineAsync(ct));
    }

    private async Task<bool> CanAsync(int functionCode, CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        return user != null && await _authorization.HasAsync(user, functionCode, ct);
    }
}
