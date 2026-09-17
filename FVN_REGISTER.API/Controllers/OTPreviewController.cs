using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Contract.Requests.OT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FVN_REGISTER.API.Controllers;

/// <summary>Preview approval hierarchy without creating the OT request.</summary>
[Authorize]
[ApiController]
[Route("api/OT")]
public sealed class OTPreviewController : ControllerBase
{
    private readonly IApprovalProvider<OTRequestSubject> _provider;

    public OTPreviewController(IApprovalProvider<OTRequestSubject> provider)
        => _provider = provider;

    [HttpPost("preview")]
    public async Task<IActionResult> Preview([FromBody] OTRequestUpsertDto model, CancellationToken ct)
    {
        if (model == null)
            return BadRequest();

        var totalHours = model.Employees?.Sum(x => x.OTHours) ?? 0m;
        var context = ApprovalBuildContext.ForOT(
            employeeCode: model.EmployeeCode ?? string.Empty,
            deptCode: model.DeptCode ?? string.Empty,
            positionCode: model.PositionCode ?? string.Empty,
            totalOTHours: totalHours,
            otTypeCode: model.OTTypeCode ?? string.Empty);

        var steps = await _provider.BuildHierarchyAsync(context, ct);
        return Ok(steps);
    }
}
