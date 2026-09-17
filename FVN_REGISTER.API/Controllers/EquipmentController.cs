using FVN_REGISTER.Application.Interfaces.Equipment;
using FVN_REGISTER.Contract.Dtos.Equipment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FVN_REGISTER.API.Controllers;

[ApiController]
[Route("api/equipment")]
[Authorize]
public sealed class EquipmentController : ControllerBase
{
    private readonly IEquipmentService _service;
    private readonly IEquipmentQrCodeService _qr;
    private readonly IConfiguration _configuration;
    public EquipmentController(IEquipmentService service, IEquipmentQrCodeService qr, IConfiguration configuration) { _service = service; _qr = qr; _configuration = configuration; }
    [HttpGet("access")] public async Task<ActionResult<bool>> Access(CancellationToken ct) => Ok(await _service.HasModuleAccessAsync(ct));
    [HttpGet("approvers")] public async Task<ActionResult<List<EquipmentApproverDto>>> Approvers([FromQuery] string deptCode, CancellationToken ct) => Ok(await _service.GetApproversAsync(deptCode, ct));
    [HttpPost("registrations")] public async Task<ActionResult<EquipmentRequestDto>> CreateRegistration([FromBody] CreateEquipmentRegistrationDto request, CancellationToken ct) => Ok(await _service.CreateRegistrationDraftAsync(request, ct));
    [HttpPost("registrations/{id:int}/submit")] public async Task<ActionResult<EquipmentRequestDto>> SubmitRegistration(int id, CancellationToken ct) => Ok(await _service.SubmitRegistrationAsync(id, ct));
    [HttpGet("registrations/mine")] public async Task<ActionResult<List<EquipmentRequestDto>>> Mine(CancellationToken ct) => Ok(await _service.GetMineAsync(ct));
    [HttpPost("repairs")] public async Task<ActionResult<EquipmentRequestDto>> CreateRepair([FromBody] CreateEquipmentRepairDto request, CancellationToken ct) => Ok(await _service.CreateRepairDraftAsync(request, ct));
    [HttpPost("repairs/{id:int}/submit")] public async Task<ActionResult<EquipmentRequestDto>> SubmitRepair(int id, CancellationToken ct) => Ok(await _service.SubmitRepairAsync(id, ct));
    [HttpGet("scan/{qrToken}")] public async Task<ActionResult<EquipmentAssetDto>> Scan(string qrToken, CancellationToken ct) => Ok(await _service.ScanAsync(qrToken, ct));
    [HttpGet("assets/{id:int}")] public async Task<ActionResult<EquipmentAssetDto>> Asset(int id, CancellationToken ct) { var result = await _service.GetAssetAsync(id, ct); return result == null ? NotFound() : Ok(result); }
    [HttpGet("qr/{qrToken}/image")]
    [AllowAnonymous]
    public ActionResult QrImage(string qrToken)
    {
        if (string.IsNullOrWhiteSpace(qrToken)) return BadRequest();
        var siteUrl = (_configuration["SiteUrl"] ?? $"{Request.Scheme}://{Request.Host}").TrimEnd('/');
        var payload = $"{siteUrl}/equipment/scan/{Uri.EscapeDataString(qrToken)}";
        return File(_qr.CreatePng(payload), "image/png");
    }
}
