using System.Text.Json;
using FVN_REGISTER.Application.Interfaces.Equipment;
using AppAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Equipment;
using FVN_REGISTER.Contract.Dtos.EquipmentImport;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    private readonly FVNWEBAPPContext _db;

    public EquipmentController(IEquipmentService service, IEquipmentImportService import, IEquipmentQrCodeService qr, IConfiguration configuration, ICurrentUserService currentUser, AppAuthorizationService authorization, FVNWEBAPPContext db)
    {
        _service = service; _import = import; _qr = qr; _configuration = configuration; _currentUser = currentUser; _authorization = authorization; _db = db;
    }

    [HttpGet("actions")]
    public async Task<ActionResult<EquipmentActionAccessDto>> Actions(CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null) return Unauthorized();
        if (!await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentView, ct)) return Forbid();
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
            History = await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentHistory, ct),
            InspectionManage = await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionManage, ct),
            InspectionExecute = await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionExecute, ct),
            InspectionApprove = await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionApprove, ct),
            InspectionReport = await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentInspectionReport, ct)
        });
    }

    [HttpGet("access")]
    public async Task<ActionResult<ApiResponse<bool>>> Access(CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentView, ct)) return Forbid();
        var result = await _service.HasModuleAccessAsync(ct);
        var response = ApiResponse<bool>.FromResult(result);
        return result.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("assignment-employees")]
    public async Task<ActionResult<ApiResponse<List<EquipmentHandoverEmployeeOptionDto>>>> AssignmentEmployees([FromQuery] string? deptCode, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentCreate, ct)) return Forbid();
        var result = await _service.GetAssignmentEmployeesAsync(deptCode, ct);
        return result.IsSuccess ? Ok(ApiResponse<List<EquipmentHandoverEmployeeOptionDto>>.FromResult(result)) : BadRequest(ApiResponse<List<EquipmentHandoverEmployeeOptionDto>>.FromResult(result));
    }

    [HttpGet("approvers")]
    public async Task<ActionResult<ApiResponse<List<EquipmentApproverDto>>>> Approvers([FromQuery] string deptCode, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentView, ct)) return Forbid();
        var result = await _service.GetApproversAsync(deptCode.Trim().ToUpperInvariant(), ct);
        var response = ApiResponse<List<EquipmentApproverDto>>.FromResult(result);
        return result.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("registrations")]
    public async Task<ActionResult<ApiResponse<EquipmentRequestDto>>> CreateRegistration([FromBody] CreateEquipmentRegistrationDto request, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentCreate, ct)) return Forbid();
        var result = await _service.CreateRegistrationDraftAsync(request, ct);
        var response = ApiResponse<EquipmentRequestDto>.FromResult(result);
        return result.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("registrations/{id:int}/submit")]
    public async Task<ActionResult<ApiResponse<EquipmentRequestDto>>> SubmitRegistration(int id, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentEdit, ct)) return Forbid();
        var result = await _service.SubmitRegistrationAsync(id, ct);
        var response = ApiResponse<EquipmentRequestDto>.FromResult(result);
        return result.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("assigned-to-me")]
    public async Task<ActionResult<ApiResponse<List<EquipmentAssetDto>>>> AssignedToMe(CancellationToken ct)
    {
        var canView = await CanAsync(SecurityFunctionCodes.EquipmentView, ct);
        var canRepair = await CanAsync(SecurityFunctionCodes.EquipmentRepair, ct);
        var user = _currentUser.GetCurrentUser();
        var assigned = user != null && !string.IsNullOrWhiteSpace(user.EmployeeCode) &&
            await _db.EquipmentAssets.AsNoTracking().AnyAsync(x => x.IsActive == true &&
                (x.ResponsibleEmployeeCode == user.EmployeeCode || x.OperatingResponsibleEmployeeCode == user.EmployeeCode), ct);
        if (!canView && !canRepair && !assigned) return Forbid();
        var result = await _service.GetMyAssignedAssetsAsync(ct);
        return result.IsSuccess ? Ok(ApiResponse<List<EquipmentAssetDto>>.FromResult(result)) : BadRequest(ApiResponse<List<EquipmentAssetDto>>.FromResult(result));
    }

    [HttpGet("registrations/mine")]
    public async Task<ActionResult<ApiResponse<List<EquipmentRequestDto>>>> Mine(CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentView, ct)) return Forbid();
        var result = await _service.GetMineAsync(ct);
        var response = ApiResponse<List<EquipmentRequestDto>>.FromResult(result);
        return result.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("repairs/assignees")]
    public async Task<ActionResult<ApiResponse<List<EquipmentHandoverEmployeeOptionDto>>>> RepairAssignees([FromQuery] string? deptCode, CancellationToken ct)
    {
        var canRepair = await CanAsync(SecurityFunctionCodes.EquipmentRepair, ct);
        var user = _currentUser.GetCurrentUser();
        var assigned = user != null && !string.IsNullOrWhiteSpace(user.EmployeeCode) && await _db.EquipmentAssets.AsNoTracking().AnyAsync(x => x.IsActive == true && (x.ResponsibleEmployeeCode == user.EmployeeCode || x.OperatingResponsibleEmployeeCode == user.EmployeeCode), ct);
        if (!canRepair && !assigned) return Forbid();
        var result = await _service.GetRepairAssigneesAsync(deptCode, ct);
        return result.IsSuccess ? Ok(ApiResponse<List<EquipmentHandoverEmployeeOptionDto>>.FromResult(result)) : BadRequest(ApiResponse<List<EquipmentHandoverEmployeeOptionDto>>.FromResult(result));
    }

    [HttpPost("repairs")]
    public async Task<ActionResult<ApiResponse<EquipmentRequestDto>>> CreateRepair([FromBody] CreateEquipmentRepairDto request, CancellationToken ct)
    {
        if (!await CanRepairOrAssignedAssetAsync(request.AssetId, ct)) return Forbid();
        var result = await _service.CreateRepairDraftAsync(request, ct);
        var response = ApiResponse<EquipmentRequestDto>.FromResult(result);
        return result.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("repairs/{id:int}/complete")]
    public async Task<ActionResult<ApiResponse<EquipmentRequestDto>>> CompleteRepair(int id, [FromBody] string feedback, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentRepair, ct)) return Forbid();
        var result = await _service.CompleteRepairAsync(id, feedback, ct);
        var response = ApiResponse<EquipmentRequestDto>.FromResult(result);
        return result.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("repairs/{id:int}/submit")]
    public async Task<ActionResult<ApiResponse<EquipmentRequestDto>>> SubmitRepair(int id, CancellationToken ct)
    {
        if (!await CanRepairOrOwnRepairDraftAsync(id, ct)) return Forbid();
        var result = await _service.SubmitRepairAsync(id, ct);
        var response = ApiResponse<EquipmentRequestDto>.FromResult(result);
        return result.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("handover/employees")]
    public async Task<ActionResult<ApiResponse<List<EquipmentHandoverEmployeeOptionDto>>>> HandoverEmployees([FromQuery] string? deptCode, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentTransfer, ct)) return Forbid();
        var result = await _service.GetHandoverEmployeesAsync(deptCode, ct);
        var response = ApiResponse<List<EquipmentHandoverEmployeeOptionDto>>.FromResult(result);
        return result.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("handover/candidates")]
    public async Task<ActionResult<ApiResponse<List<EquipmentHandoverCandidateDto>>>> HandoverCandidates([FromQuery] string oldEmployeeCode, [FromQuery] string? deptCode, CancellationToken ct)
    {
        var canTransfer = await CanAsync(SecurityFunctionCodes.EquipmentTransfer, ct);
        var canCreateAccessChange = await CanAsync(SecurityFunctionCodes.SecurityAccessChangeCreate, ct);
        if (!canTransfer && !canCreateAccessChange) return Forbid();
        var result = await _service.GetHandoverCandidatesAsync(oldEmployeeCode, deptCode, ct);
        var response = ApiResponse<List<EquipmentHandoverCandidateDto>>.FromResult(result);
        return result.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("handover")]
    public ActionResult<ApiResponse<EquipmentHandoverResultDto>> Handover([FromBody] EquipmentHandoverRequest request, CancellationToken ct)
    {
        // Equipment responsibility/approver changes must come from the cross-module
        // access-change approval workflow. The service method remains reusable by
        // the IT execution path after final approval, but this public endpoint must
        // not provide a bypass around approval.
        return BadRequest(ApiResponse<EquipmentHandoverResultDto>.Fail(
            "Bàn giao thiết bị phải được lập qua Phiếu thay đổi quyền và chỉ IT thực hiện sau khi phiếu được duyệt đủ cấp."));
    }

    [HttpGet("reports")]
    public async Task<ActionResult<ApiResponse<List<EquipmentReportRowDto>>>> Report([FromQuery] EquipmentReportFilterDto filter, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentExport, ct)) return Forbid();
        var result = await _service.GetReportAsync(filter, ct);
        return result.IsSuccess ? Ok(ApiResponse<List<EquipmentReportRowDto>>.FromResult(result)) : BadRequest(ApiResponse<List<EquipmentReportRowDto>>.FromResult(result));
    }

    [HttpGet("reports/export")]
    public async Task<IActionResult> ExportReport([FromQuery] EquipmentReportFilterDto filter, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentExport, ct)) return Forbid();
        var result = await _service.ExportReportAsync(filter, ct);
        if (!result.IsSuccess || result.Data == null) return BadRequest(ApiResponse<byte[]>.FromResult(result));
        return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"EquipmentReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    [HttpGet("scan/{qrToken}")]
    public async Task<ActionResult<ApiResponse<EquipmentAssetDto>>> Scan(string qrToken, CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        var canQr = await CanAsync(SecurityFunctionCodes.EquipmentQR, ct);
        var assigned = user != null && !string.IsNullOrWhiteSpace(user.EmployeeCode) && await _db.EquipmentAssets.AsNoTracking().AnyAsync(x => x.IsActive == true && x.IsQrActive && x.QrToken == qrToken && (x.ResponsibleEmployeeCode == user.EmployeeCode || x.OperatingResponsibleEmployeeCode == user.EmployeeCode), ct);
        if (!canQr && !assigned) return Forbid();
        var result = await _service.ScanAsync(qrToken, ct);
        return result.IsSuccess ? Ok(ApiResponse<EquipmentAssetDto>.FromResult(result)) : BadRequest(ApiResponse<EquipmentAssetDto>.FromResult(result));
    }

    [HttpGet("assets/{id:int}/history")]
    public async Task<ActionResult<ApiResponse<EquipmentAssetDto>>> AssetHistory(int id, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentHistory, ct)) return Forbid();
        var result = await _service.GetAssetAsync(id, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<EquipmentAssetDto>.FromResult(result));
        return result.Data == null ? NotFound(ApiResponse<EquipmentAssetDto>.Fail("Không tìm thấy thiết bị.", 404)) : Ok(ApiResponse<EquipmentAssetDto>.FromResult(result));
    }

    [HttpGet("assets/{id:int}")]
    public async Task<ActionResult<ApiResponse<EquipmentAssetDto>>> Asset(int id, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentView, ct)) return Forbid();
        var result = await _service.GetAssetAsync(id, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<EquipmentAssetDto>.FromResult(result));
        return result.Data == null ? NotFound(ApiResponse<EquipmentAssetDto>.Fail("Không tìm thấy thiết bị.", 404)) : Ok(ApiResponse<EquipmentAssetDto>.FromResult(result));
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
        return Ok(await _import.GetFieldDefinitionsAsync(deptCode.Trim().ToUpperInvariant(), ct));
    }

    [HttpPut("schema")]
    public async Task<ActionResult<EquipmentFieldDefinitionDto>> SaveSchema([FromBody] SaveEquipmentFieldDefinitionRequest request, CancellationToken ct)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentImport, ct)) return Forbid();
        return Ok(await _import.SaveFieldDefinitionAsync(request, ct));
    }

    [HttpPost("import")]
    [RequestSizeLimit(25_000_000)]
    public async Task<ActionResult<EquipmentImportBatchDto>> Import(IFormFile file, [FromQuery] string deptCode, [FromQuery] bool assignToEmployee = false, CancellationToken ct = default)
    {
        if (!await CanAsync(SecurityFunctionCodes.EquipmentImport, ct)) return Forbid();
        if (file == null || file.Length == 0) return BadRequest("File Excel rỗng.");
        deptCode = deptCode.Trim().ToUpperInvariant();
        await using var stream = file.OpenReadStream();
        return Ok(await _import.StageExcelAsync(deptCode, file.FileName, stream, assignToEmployee, ct));
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
        var batch = await _db.EquipmentImportBatches.AsNoTracking().FirstOrDefaultAsync(x => x.Id == batchId && x.IsActive == true, ct);
        if (batch == null) return NotFound();
        var rows = await _db.EquipmentImportRows.AsNoTracking().Where(x => x.BatchId == batchId && x.Status == "Valid").ToListAsync(ct);
        var codes = rows.Select(x => TryGetEquipmentCode(x.RawJson)).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!).ToList();
        var duplicateInFile = codes.GroupBy(x => x, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Select(g => g.Key).Take(10).ToList();
        if (duplicateInFile.Count > 0) return BadRequest($"File có mã thiết bị trùng: {string.Join(", ", duplicateInFile)}.");
        var existing = await _db.EquipmentAssets.AsNoTracking().Where(x => codes.Contains(x.EquipmentCode)).Select(x => x.EquipmentCode).ToListAsync(ct);
        if (existing.Count > 0) return Conflict($"Thiết bị đã tồn tại: {string.Join(", ", existing.Take(10))}.");
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

    private async Task<bool> CanRepairOrAssignedAssetAsync(int assetId, CancellationToken ct)
    {
        if (await CanAsync(SecurityFunctionCodes.EquipmentRepair, ct)) return true;
        var user = _currentUser.GetCurrentUser();
        if (user == null || string.IsNullOrWhiteSpace(user.EmployeeCode)) return false;
        return await _db.EquipmentAssets.AsNoTracking().AnyAsync(x =>
            x.Id == assetId &&
            x.IsActive == true &&
            (x.ResponsibleEmployeeCode == user.EmployeeCode ||
             x.OperatingResponsibleEmployeeCode == user.EmployeeCode), ct);
    }

    private async Task<bool> CanRepairOrOwnRepairDraftAsync(int requestId, CancellationToken ct)
    {
        if (await CanAsync(SecurityFunctionCodes.EquipmentRepair, ct)) return true;
        var user = _currentUser.GetCurrentUser();
        if (user == null || string.IsNullOrWhiteSpace(user.EmployeeCode)) return false;
        return await _db.EquipmentRequests.AsNoTracking().AnyAsync(x =>
            x.Id == requestId &&
            x.IsActive == true &&
            x.RequestKind == EquipmentRequestKind.Repair &&
            x.EmployeeCode == user.EmployeeCode &&
            (x.RequestStatus == ApprovalStatus.Draft ||
             x.RequestStatus == ApprovalStatus.NeedsRevision), ct);
    }

    private static string? TryGetEquipmentCode(string rawJson)
    {
        try
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, string?>>(rawJson);
            if (data == null) return null;
            foreach (var pair in data)
            {
                var key = new string(pair.Key.Trim().Normalize(System.Text.NormalizationForm.FormD).Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).Select(char.ToLowerInvariant).ToArray());
                key = new string(key.Where(char.IsLetterOrDigit).ToArray());
                if (key is "equipmentcode" or "matb" or "matthietbi" or "matasan" or "assetcode") return pair.Value?.Trim();
            }
        }
        catch (JsonException) { }
        return null;
    }
}
