using FVN_REGISTER.Application.Interfaces.Trips;
using FVN_REGISTER.Contract.Dtos.Trips;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FVN_REGISTER.API.Controllers;

[ApiController]
[Route("api/trips")]
[Authorize]
public sealed class TripController : ControllerBase
{
    private readonly ITripService _service;

    public TripController(ITripService service) => _service = service;

    [HttpPost]
    public async Task<ActionResult<TripRequestDto>> Create(
        [FromBody] CreateTripRequestDto request,
        CancellationToken ct)
        => Ok(await _service.CreateDraftAsync(request, ct));

    [HttpPost("{id:int}/submit")]
    public async Task<ActionResult<TripRequestDto>> Submit(int id, CancellationToken ct)
        => Ok(await _service.SubmitAsync(id, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TripRequestDto>> Get(int id, CancellationToken ct)
    {
        var result = await _service.GetAsync(id, ct);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<TripRequestDto>>> Mine(CancellationToken ct)
        => Ok(await _service.GetMineAsync(ct));
}
