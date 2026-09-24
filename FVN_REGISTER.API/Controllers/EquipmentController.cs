using FVN_REGISTER.Application.Interfaces.Equipment;
using AppAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Equipment;
using FVN_REGISTER.Contract.Dtos.EquipmentImport;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;


namespace FVN_REGISTER.API.Controllers;

[ApiController]
[Route("api/equipment")]
[Authorize]
public sealed class EquipmentController : ControllerBase
{
    private readonly IEquipmentService _service;
    private readonly IEquipmentImportService _import;
    private readonly IEquipmentQrCodeService _qr;
    private readonly IConfiguration _configuration;
    private readonly ICurrentUserService _currentUser;
    private readonly AppAuthorizationService _authorization;

    public EquipmentController(
        IEquipmentService service,
        IEquipmentImportService import,
        IEquipmentQrCodeService qr,
        IConfiguration configuration,
        ICurrentUserService currentUser,
        AppAuthorizationService authorization)
    {
        _service = service;
        _import = import;
        _qr = qr;
        _configuration = configuration;
        _currentUser = currentUser;
        _authorization = authorization;
    }

    [HttpGet("actions")]
    public async Task<ActionResult<EquipmentActionAccessDto>> Actions(CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null) return Unauthorized();

        if (!await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentView, ct))
            return Forbid();

        return Ok(new EquipmentActionAccessDto
        {
            View = true,
            Create = await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentCreate, ct),
            Edit = await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentEdit, ct),
            Assign = await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentAssign, ct),
            Transfer = await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentTransfer, ct),
            Return = await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentReturn, ct),
            Repair = await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentRepair, ct),
            Liquidate = await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentLiquidate, ct),
            Approve = await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentApprove, ct),
            Import = await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentImport, ct),
            Export = await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentExport, ct),
            QR = await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentQR, ct),
            History = await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentHistory, ct)
        });
    }

    [HttpGet("access")]
    public async Task<ActionResult<bool>> Access(CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentView, ct)) return Forbid();
        var result = await _service.HasModuleAccessAsync(ct);
        var response = ApiResponse<bool>.FromResult(result);
        return result.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("approvers")]
    public async Task<ActionResult<List<EquipmentApproverDto>>> Approvers([FromQuery] string deptCode, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentView, ct)) return Forbid();
        var result = await _service.GetApproversAsync(deptCode, ct);
        var response = ApiResponse<List<EquipmentApproverDto>>.FromResult(result);
        return result.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("registrations")]
    public async Task<ActionResult<EquipmentRequestDto>> CreateRegistration([FromBody] CreateEquipmentRegistrationDto request, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentCreate, ct)) return Forbid();
        var result = await _service.CreateRegistrationDraftAsync(request, ct);
        var response = ApiResponse<EquipmentRequestDto>.FromResult(result);
        return result.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("registrations/{id:int}/submit")]
    public async Task<ActionResult<EquipmentRequestDto>> SubmitRegistration(int id, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentEdit, ct)) return Forbid();
        var result = await _service.SubmitRegistrationAsync(id, ct);
        var response = ApiResponse<EquipmentRequestDto>.FromResult(result);
        return result.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("registrations/mine")]
    public async Task<ActionResult<List<EquipmentRequestDto>>> Mine(CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentView, ct)) return Forbid();
        var result = await _service.GetMineAsync(ct);
        var response = ApiResponse<List<EquipmentRequestDto>>.FromResult(result);
        return result.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("repairs")]
    public async Task<ActionResult<EquipmentRequestDto>> CreateRepair([FromBody] CreateEquipmentRepairDto request, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentRepair, ct)) return Forbid();
        var result = await _service.CreateRepairDraftAsync(request, ct);
        var response = ApiResponse<EquipmentRequestDto>.FromResult(result);
        return result.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("repairs/{id:int}/submit")]
    public async Task<ActionResult<EquipmentRequestDto>> SubmitRepair(int id, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentRepair, ct)) return Forbid();
        var result = await _service.SubmitRepairAsync(id, ct);
        var response = ApiResponse<EquipmentRequestDto>.FromResult(result);
        return result.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("scan/{qrToken}")]
    public async Task<ActionResult<EquipmentAssetDto>> Scan(string qrToken, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentQR, ct)) return Forbid();
        var result = await _service.ScanAsync(qrToken, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<EquipmentAssetDto>.FromResult(result));
        return Ok(ApiResponse<EquipmentAssetDto>.FromResult(result));
    }

    [HttpGet("assets/{id:int}/history")]
    public async Task<ActionResult<EquipmentAssetDto>> AssetHistory(int id, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentHistory, ct)) return Forbid();
        var result = await _service.GetAssetAsync(id, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<EquipmentAssetDto>.FromResult(result));
        return result.Data == null
            ? NotFound(ApiResponse<EquipmentAssetDto>.Fail("Không tìm thấy thiết bị.", 404))
            : Ok(ApiResponse<EquipmentAssetDto>.FromResult(result));
    }

    [HttpGet("assets/{id:int}")]
    public async Task<ActionResult<EquipmentAssetDto>> Asset(int id, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentView, ct)) return Forbid();
        var result = await _service.GetAssetAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<EquipmentAssetDto>.FromResult(result));

        return result.Data == null
            ? NotFound(ApiResponse<EquipmentAssetDto>.Fail("Không tìm thấy thiết bị.", 404))
            : Ok(ApiResponse<EquipmentAssetDto>.FromResult(result));
    }

    [HttpGet("import/access")]
    public async Task<ActionResult<bool>> ImportAccess(CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentImport, ct)) return Forbid();
        return Ok(true);
    }

    [HttpGet("schema/{deptCode}")]
    public async Task<ActionResult<List<EquipmentFieldDefinitionDto>>> Schema(string deptCode, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentImport, ct)) return Forbid();
        return Ok(await _import.GetFieldDefinitionsAsync(deptCode, ct));
    }

    [HttpPut("schema")]
    public async Task<ActionResult<EquipmentFieldDefinitionDto>> SaveSchema([FromBody] SaveEquipmentFieldDefinitionRequest request, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentImport, ct)) return Forbid();
        return Ok(await _import.SaveFieldDefinitionAsync(request, ct));
    }

    [HttpPost("import")]
    [RequestSizeLimit(25_000_000)]
    public async Task<ActionResult<EquipmentImportBatchDto>> Import(IFormFile file, [FromQuery] string deptCode, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentImport, ct)) return Forbid();
        if (file == null || file.Length == 0) return BadRequest("File Excel rỗng.");
        await using var stream = file.OpenReadStream();
        return Ok(await _import.StageExcelAsync(deptCode, file.FileName, stream, ct));
    }

    [HttpGet("import/{batchId:int}")]
    public async Task<ActionResult<EquipmentImportBatchDto>> ImportBatch(int batchId, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentImport, ct)) return Forbid();
        var result = await _import.GetBatchAsync(batchId, ct);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("import/{batchId:int}/commit")]
    public async Task<ActionResult<EquipmentImportCommitResultDto>> CommitImport(int batchId, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentImport, ct)) return Forbid();
        return Ok(await _import.CommitAsync(batchId, ct));
    }

    [HttpGet("qr/{qrToken}/image")]
    [AllowAnonymous]
    public ActionResult QrImage(string qrToken)
    {
        if (string.IsNullOrWhiteSpace(qrToken)) return BadRequest();
        var siteUrl = (_configuration["SiteUrl"] ?? $"{Request.Scheme}://{Request.Host}").TrimEnd('/');
        var payload = $"{siteUrl}/equipment/scan/{Uri.EscapeDataString(qrToken)}";
        return File(_qr.CreatePng(payload), "image/png");
    }

    private async Task<bool> CanAsync(int functionCode, CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        return user != null && await _authorization.HasAsync(user, functionCode, ct);
    }
}
