using FVN_REGISTER.Application.Interfaces.Equipment;
using FVN_REGISTER.Contract.Dtos.Equipment;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Interfaces.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FVN_REGISTER.API.Controllers;

[ApiController]
[Route("api/equipment-inspections")]
[Authorize]
public sealed class EquipmentInspectionsController : ControllerBase
{
    private readonly IEquipmentInspectionService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationService _authorization;

    public EquipmentInspectionsController(IEquipmentInspectionService service, ICurrentUserService currentUser, IAuthorizationService authorization)
    {
        _service = service; _currentUser = currentUser; _authorization = authorization;
    }

    [HttpGet("registered-assets")]
    public async Task<IActionResult> RegisteredAssets([FromQuery] string? deptCode, CancellationToken ct)
        => From(await _service.GetRegisteredAssetsAsync(deptCode, ct));

    [HttpGet("templates")]
    public async Task<IActionResult> Templates([FromQuery] string? deptCode, CancellationToken ct)
        => From(await _service.GetTemplatesAsync(deptCode, ct));

    [HttpGet("templates/{id:int}")]
    public async Task<IActionResult> Template(int id, CancellationToken ct)
        => From(await _service.GetTemplateAsync(id, ct));

    [HttpPost("templates/import")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> ImportTemplateExcel(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0) return BadRequest(ApiResponse<object>.Fail("File Excel rỗng."));
        await using var stream = file.OpenReadStream();
        return From(await _service.ImportTemplateExcelAsync(file.FileName, stream, ct));
    }

    [HttpPost("templates")]
    public async Task<IActionResult> SaveTemplate([FromBody] EquipmentInspectionTemplateUpsertRequest request, CancellationToken ct)
        => From(await _service.SaveTemplateAsync(request, ct));

    [HttpPost("templates/{id:int}/clone")]
    public async Task<IActionResult> CloneTemplate(int id, [FromBody] EquipmentInspectionTemplateCloneRequest request, CancellationToken ct)
        => From(await _service.CloneTemplateAsync(id, request.TemplateName, ct));

    [HttpPost("assignments")]
    public async Task<IActionResult> Assign([FromBody] EquipmentInspectionAssignmentRequest request, CancellationToken ct)
        => From(await _service.AssignAsync(request, ct));

    [HttpGet("assignments")]
    public async Task<IActionResult> Assignments([FromQuery] int? equipmentId, CancellationToken ct)
        => From(await _service.GetAssignmentsAsync(equipmentId, ct));

    [HttpGet("tasks")]
    public async Task<IActionResult> Tasks([FromQuery] bool includeCompleted = false, CancellationToken ct = default)
        => From(await _service.GetMyTasksAsync(includeCompleted, ct));

    [HttpGet("tasks/{id:int}")]
    public async Task<IActionResult> Task(int id, CancellationToken ct)
        => From(await _service.GetTaskAsync(id, ct));

    [HttpPost("tasks/submit")]
    public async Task<IActionResult> Submit([FromBody] EquipmentInspectionSubmitRequest request, CancellationToken ct)
        => From(await _service.SubmitAsync(request, ct));

    [HttpPost("tasks/{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
        => From(await _service.ApproveAsync(id, ct));

    [HttpPost("tasks/{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] string reason, CancellationToken ct)
        => From(await _service.RejectAsync(id, reason, ct));

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? deptCode, CancellationToken ct)
        => From(await _service.DashboardAsync(from?.Date ?? DateTime.Today.AddDays(-30), to?.Date ?? DateTime.Today, deptCode, ct));

    [HttpPost("tasks/{taskId:int}/evidence")]
    [RequestSizeLimit(12_000_000)]
    public async Task<IActionResult> Evidence(int taskId, [FromQuery] int? itemResultId, [FromQuery] int? itemId, IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0) return BadRequest(ApiResponse<object>.Fail("Hình ảnh rỗng."));
        await using var stream = file.OpenReadStream();
        var result = await _service.AddEvidenceAsync(taskId, itemResultId, itemId, file.FileName, file.ContentType, stream, ct);
        return From(result);
    }

    [HttpGet("evidence/{id:int}")]
    public async Task<IActionResult> Evidence(int id, CancellationToken ct)
    {
        var result = await _service.OpenEvidenceAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Fail(result.Message ?? "Không tìm thấy hình ảnh.", 404));
        return File(result.Data.Content, result.Data.ContentType, result.Data.FileName);
    }

    private IActionResult From<T>(FVN_REGISTER.Contract.Utils.ServiceResult<T> result)
    {
        var response = ApiResponse<T>.FromResult(result);
        if (result.IsSuccess) return Ok(response);
        return BadRequest(response);
    }
}