using FVN_REGISTER.Application.Interfaces.Actions;
using FVN_REGISTER.Contract.Dtos.Actions;
using FVN_REGISTER.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Actions;

public sealed class ActionItemService : IActionItemService
{
    private readonly FVNWEBAPPContext _db;

    public ActionItemService(FVNWEBAPPContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ActionItemDto>> GetMineAsync(
        string employeeCode,
        int userId,
        bool includeCompleted = false,
        CancellationToken cancellationToken = default)
    {
        var employeeId = await ResolveEmployeeIdAsync(employeeCode, cancellationToken);
        var query = _db.ActionItems
            .AsNoTracking()
            .Where(x => x.IsActive != false
                && x.AssignedToEmployeeId == employeeId
                && (x.AssignedToUserId == null || x.AssignedToUserId == userId));

        if (!includeCompleted)
            query = query.Where(x => x.Status == ActionItemStatus.Open
                || x.Status == ActionItemStatus.InProgress);

        return await query
            .OrderBy(x => x.Status == ActionItemStatus.Open ? 0 : 1)
            .ThenByDescending(x => x.Priority)
            .ThenBy(x => x.DueAt)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new ActionItemDto
            {
                ActionId = x.ActionId,
                ModuleCode = x.ModuleCode,
                SourceId = x.SourceId,
                EmployeeId = x.EmployeeId,
                AssignedToEmployeeId = x.AssignedToEmployeeId,
                WorkDate = x.WorkDate,
                ActionType = x.ActionType,
                Title = x.Title,
                Summary = x.Summary,
                Severity = x.Severity,
                Priority = x.Priority,
                Status = (byte)x.Status,
                DueAt = x.DueAt,
                DetailRoute = x.DetailRoute,
                ReferenceNo = x.ReferenceNo,
                CreatedAt = x.CreatedAt,
                CompletedAt = x.CompletedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ActionCountDto> GetCountAsync(
        string employeeCode,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var employeeId = await ResolveEmployeeIdAsync(employeeCode, cancellationToken);
        var counts = await _db.ActionItems
            .AsNoTracking()
            .Where(x => x.IsActive != false
                && x.AssignedToEmployeeId == employeeId
                && (x.AssignedToUserId == null || x.AssignedToUserId == userId)
                && (x.Status == ActionItemStatus.Open || x.Status == ActionItemStatus.InProgress))
            .GroupBy(x => x.Status)
            .Select(x => new { Status = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        return new ActionCountDto
        {
            OpenCount = counts.FirstOrDefault(x => x.Status == ActionItemStatus.Open)?.Count ?? 0,
            InProgressCount = counts.FirstOrDefault(x => x.Status == ActionItemStatus.InProgress)?.Count ?? 0
        };
    }

    public async Task<ActionItemDto?> GetAsync(
        string employeeCode,
        int userId,
        Guid actionId,
        CancellationToken cancellationToken = default)
    {
        var employeeId = await ResolveEmployeeIdAsync(employeeCode, cancellationToken);
        return await _db.ActionItems
            .AsNoTracking()
            .Where(x => x.ActionId == actionId
                && x.IsActive != false
                && x.AssignedToEmployeeId == employeeId
                && (x.AssignedToUserId == null || x.AssignedToUserId == userId))
            .Select(x => new ActionItemDto
            {
                ActionId = x.ActionId,
                ModuleCode = x.ModuleCode,
                SourceId = x.SourceId,
                EmployeeId = x.EmployeeId,
                AssignedToEmployeeId = x.AssignedToEmployeeId,
                WorkDate = x.WorkDate,
                ActionType = x.ActionType,
                Title = x.Title,
                Summary = x.Summary,
                Severity = x.Severity,
                Priority = x.Priority,
                Status = (byte)x.Status,
                DueAt = x.DueAt,
                DetailRoute = x.DetailRoute,
                ReferenceNo = x.ReferenceNo,
                CreatedAt = x.CreatedAt,
                CompletedAt = x.CompletedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> CompleteAsync(string employeeCode, int userId, Guid actionId, CancellationToken cancellationToken = default)
    {
        var employeeId = await ResolveEmployeeIdAsync(employeeCode, cancellationToken);
        return await SetTerminalStatusAsync(employeeId, userId, actionId, ActionItemStatus.Completed, cancellationToken);
    }

    public async Task<bool> DismissAsync(string employeeCode, int userId, Guid actionId, CancellationToken cancellationToken = default)
    {
        var employeeId = await ResolveEmployeeIdAsync(employeeCode, cancellationToken);
        return await SetTerminalStatusAsync(employeeId, userId, actionId, ActionItemStatus.Dismissed, cancellationToken);
    }

    private async Task<bool> SetTerminalStatusAsync(
        int employeeId,
        int userId,
        Guid actionId,
        ActionItemStatus target,
        CancellationToken cancellationToken)
    {
        var entity = await _db.ActionItems
            .FirstOrDefaultAsync(x => x.ActionId == actionId
                && x.IsActive != false
                && x.AssignedToEmployeeId == employeeId
                && (x.AssignedToUserId == null || x.AssignedToUserId == userId), cancellationToken);

        if (entity is null)
            return false;

        if (entity.Status == target)
            return true;

        if (entity.Status is ActionItemStatus.Completed
            or ActionItemStatus.Dismissed
            or ActionItemStatus.Expired
            or ActionItemStatus.Cancelled)
            return false;

        entity.Status = target;

        if (target == ActionItemStatus.Completed)
            entity.CompletedAt = DateTime.Now;
        else if (target == ActionItemStatus.Dismissed)
            entity.DismissedAt = DateTime.Now;

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<int> ResolveEmployeeIdAsync(string employeeCode, CancellationToken cancellationToken)
    {
        return await _db.Employees
            .AsNoTracking()
            .Where(x => x.IsActive != false && x.EmployeeCode == employeeCode)
            .Select(x => (int?)x.Id)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy nhân viên của tài khoản hiện tại.");
    }
}
