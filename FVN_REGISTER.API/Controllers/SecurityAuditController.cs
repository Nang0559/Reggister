using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Infrastructure;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Contract.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;

namespace FVN_REGISTER.API.Controllers;

[Authorize]
[ApiController]
[Route("api/security/audit")]
public sealed class SecurityAuditController : ControllerBase
{
    private readonly FVNWEBAPPContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly AppAuthorizationService _authorization;

    public SecurityAuditController(FVNWEBAPPContext db, ICurrentUserService currentUser, AppAuthorizationService authorization)
    {
        _db = db;
        _currentUser = currentUser;
        _authorization = authorization;
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? userId,
        [FromQuery] string? action,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null) return Unauthorized();
        if (!await _authorization.HasAsync(user, SecurityFunctionCodes.SecurityAudit, ct)) return Forbid();

        page = Math.Clamp(page, 1, 500);
        pageSize = Math.Clamp(pageSize, 10, 200);
        var query = _db.AuditLogs.AsNoTracking().AsQueryable();

        if (from.HasValue) query = query.Where(x => x.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(x => x.CreatedAt < to.Value.AddDays(1));
        if (userId.HasValue) query = query.Where(x => x.UserId == userId.Value);
        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(x => x.Action == action);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => (x.UserName ?? "").Contains(search) || (x.Description ?? "").Contains(search) || x.Action.Contains(search));

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id, x.UserId, x.UserName, x.Action, x.Description,
                x.IpAddress, x.UserAgent, x.CreatedAt
            })
            .ToListAsync(ct);

        var result = items.Select(x => new SecurityAuditEntryDto
        {
            Id = x.Id, UserId = x.UserId, UserName = x.UserName, Action = x.Action,
            Description = x.Description, IpAddress = x.IpAddress, UserAgent = x.UserAgent, CreatedAt = x.CreatedAt
        }).ToList();
        return Ok(ApiResponse<List<SecurityAuditEntryDto>>.Ok(result));
    }
}
