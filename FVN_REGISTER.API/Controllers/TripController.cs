using FVN_REGISTER.Contract.Requests.Approvals;
using AppAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;
using FVN_REGISTER.Application.Interfaces.Trips;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Trips;
using FVN_REGISTER.Contract.Requests.Trips;
using FVN_REGISTER.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;

namespace FVN_REGISTER.API.Controllers;

[ApiController]
[Route("api/trips")]
[Authorize]
public sealed class TripController : ControllerBase
{
    private readonly ITripService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly AppAuthorizationService _authorization;

    public TripController(
        ITripService service,
        ICurrentUserService currentUser,
        AppAuthorizationService authorization)
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

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TripRequestDto>> Update(
        int id, [FromBody] CreateTripRequestDto request, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.TripEdit, ct)) return Forbid();
        return Ok(await _service.UpdateDraftAsync(id, request, ct));
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelTripRequestDto request, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.TripCancel, ct)) return Forbid();
        await _service.CancelAsync(id, request.Reason, ct);
        return Ok();
    }

    [HttpPost("{id:int}/submit")]
    public async Task<ActionResult<TripRequestDto>> Submit(int id, [FromBody] List<ApprovalSelectionDto>? approvalSelections, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.TripEdit, ct)) return Forbid();
        return Ok(await _service.SubmitAsync(id, approvalSelections, ct));
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
