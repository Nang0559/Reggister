using FVN_REGISTER.Application.Interfaces.FeatureOperators;
using FVN_REGISTER.Application.Interfaces.Security;
using FvnAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FVN_REGISTER.API.Controllers;

[ApiController]
[Authorize]
[Route("api/security/feature-operators")]
public sealed class FeatureOperatorAssignmentsController : ControllerBase
{
    private readonly IFeatureOperatorAssignmentService _service;
    private readonly FvnAuthorizationService _authorization;
    private readonly ICurrentUserService _currentUser;

    public FeatureOperatorAssignmentsController(
        IFeatureOperatorAssignmentService service,
        FvnAuthorizationService authorization,
        ICurrentUserService currentUser)
    {
        _service = service;
        _authorization = authorization;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int functionCode,
        [FromQuery] string resourceType,
        [FromQuery] int? resourceId,
        CancellationToken ct)
    {
        if (!await CanManageAsync(ct)) return Forbid();
        return Ok(await _service.GetAsync(functionCode, resourceType, resourceId, ct));
    }

    [HttpGet("employees")]
    public async Task<IActionResult> Employees([FromQuery] string? search, CancellationToken ct)
    {
        if (!await CanManageAsync(ct)) return Forbid();
        return Ok(await _service.GetEmployeesAsync(search, ct));
    }

    [HttpGet("resources")]
    public async Task<IActionResult> Resources([FromQuery] string resourceType, CancellationToken ct)
    {
        if (!await CanManageAsync(ct)) return Forbid();
        return Ok(await _service.GetResourcesAsync(resourceType, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Add(
        [FromBody] SaveFeatureOperatorAssignmentRequest request,
        CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null) return Unauthorized();
        if (!await CanManageAsync(ct)) return Forbid();

        return Ok(await _service.AddAsync(request, user.UserId, ct));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remove(int id, CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null) return Unauthorized();
        if (!await CanManageAsync(ct)) return Forbid();

        return Ok(await _service.RemoveAsync(id, user.UserId, ct));
    }

    private async Task<bool> CanManageAsync(CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        return user != null &&
               await _authorization.HasAsync(user, SecurityFunctionCodes.UserManagementAssignPermission, ct);
    }
}
