using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Application.Models.Calendar;
using FVN_REGISTER.Contract.Dtos.Calendar;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Calendar;

public sealed class ProjectionCalendarModuleProvider : ICalendarModuleProvider
{
    private readonly FVNWEBAPPContext _db;

    public ProjectionCalendarModuleProvider(FVNWEBAPPContext db, string moduleCode)
    {
        _db = db;
        ModuleCode = moduleCode;
    }

    public string ModuleCode { get; }

    public async Task<IReadOnlyList<CalendarItemDto>> GetItemsAsync(
        CalendarContext context,
        CancellationToken cancellationToken = default)
    {
        return await _db.CalendarProjections
            .AsNoTracking()
            .Where(x => x.IsActive != false
                && x.EmployeeId == context.EmployeeId
                && x.ModuleCode == ModuleCode
                && x.WorkDate >= context.From
                && x.WorkDate <= context.To)
            .OrderBy(x => x.WorkDate)
            .ThenByDescending(x => x.RequiresAction)
            .ThenByDescending(x => x.Severity)
            .Select(x => new CalendarItemDto
            {
                WorkDate = x.WorkDate,
                ModuleCode = x.ModuleCode,
                StatusCode = x.StatusCode,
                Marker = x.Marker,
                Summary = x.Summary,
                Severity = x.Severity,
                RequiresAction = x.RequiresAction,
                InteractionType = x.RequiresAction && x.ActionId != null ? "CONFIRMATION" : x.DetailRoute != null ? "DETAIL" : "INFO",
                ActionId = x.ActionId,
                DetailRoute = x.DetailRoute,
                SourceId = x.SourceId
            })
            .ToListAsync(cancellationToken);
    }
}
