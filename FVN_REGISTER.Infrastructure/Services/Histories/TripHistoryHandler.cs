using FVN_REGISTER.Application.Interfaces.Histories;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Contract.Dtos.Notifications;
using FVN_REGISTER.Core.Entities.Trips;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Histories;

public sealed class TripHistoryHandler : BaseHistoryHandler<F03TripRequest>
{
    protected override RequestModule ModuleKind => RequestModule.Trip;

    public TripHistoryHandler(FVNWEBAPPContext db) : base(db) { }

    protected override System.Linq.Expressions.Expression<Func<F03TripRequest, bool>> BuildIdPredicate(int id)
        => x => x.Id == id;

    public override async Task<ServiceResult<PaginationResult<HistoryItemDto>>> GetHistoryAsync(
        HistoryFilterDto filter, UserIdentityDto user, CancellationToken ct)
    {
        var q = from request in Db.TripRequests.AsNoTracking()
                join employee in Db.Employees.AsNoTracking()
                    on request.EmployeeCode equals employee.EmployeeCode into employees
                from employee in employees.DefaultIfEmpty()
                where request.IsActive == true && request.EmployeeCode == user.EmployeeCode
                select new { request, employee };

        if (filter.Year.HasValue) q = q.Where(x => x.request.StartDate.Year == filter.Year.Value);
        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<ApprovalStatus>(filter.Status, true, out var status))
            q = q.Where(x => x.request.RequestStatus == status);
        if (filter.FromDate.HasValue) q = q.Where(x => x.request.StartDate >= filter.FromDate.Value);
        if (filter.ToDate.HasValue) q = q.Where(x => x.request.EndDate <= filter.ToDate.Value);
        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var s = filter.SearchText.ToLower();
            q = q.Where(x => x.request.Destination.ToLower().Contains(s) ||
                             x.request.Purpose.ToLower().Contains(s));
        }

        var total = await q.CountAsync(ct);
        var data = await q.OrderByDescending(x => x.request.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync(ct);

        var ids = data.Select(x => x.request.Id).ToList();
        var stepsMap = await GetApprovalStepsMapAsync(ids, ct);
        var items = data.Select(x => new HistoryItemDto
        {
            Id = x.request.Id,
            Kind = RequestModule.Trip,
            EmployeeCode = x.request.EmployeeCode,
            EmployeeName = x.employee?.EmployeeName ?? string.Empty,
            DeptCode = x.employee?.DeptCode,
            SubmittedAt = x.request.CreatedAt ,
            RequestStatus = x.request.RequestStatus.ToString(),
            StatusDisplay = x.request.RequestStatus.ToDisplayName(),
            StatusColor = GetStatusColor(x.request.RequestStatus),
            Reason = x.request.Purpose,
            StartDate = x.request.StartDate,
            EndDate = x.request.EndDate,
            CanCancel = ActiveStatuses.Contains(x.request.RequestStatus),
            ApprovalSteps = stepsMap.GetValueOrDefault(x.request.Id, new())
        }).ToList();

        return ServiceResult<PaginationResult<HistoryItemDto>>.Ok(
            new PaginationResult<HistoryItemDto>(items, total, filter.Page, filter.PageSize));
    }

    public override async Task<ServiceResult<HistoryItemDetailDto>> GetDetailAsync(
        int id, UserIdentityDto user, CancellationToken ct)
    {
        var row = await (from request in Db.TripRequests.AsNoTracking()
                         join employee in Db.Employees.AsNoTracking()
                             on request.EmployeeCode equals employee.EmployeeCode into employees
                         from employee in employees.DefaultIfEmpty()
                         where request.Id == id &&
                               request.IsActive == true &&
                               request.EmployeeCode == user.EmployeeCode
                         select new { request, employee })
            .FirstOrDefaultAsync(ct);

        if (row == null)
            return ServiceResult<HistoryItemDetailDto>.Fail("Không tìm thấy đơn công tác.");

        return ServiceResult<HistoryItemDetailDto>.Ok(new HistoryItemDetailDto
        {
            Id = id,
            Kind = RequestModule.Trip.ToString(),
            RequestStatus = row.request.RequestStatus.ToString(),
            StatusDisplay = row.request.RequestStatus.ToDisplayName(),
            StatusColor = GetStatusColor(row.request.RequestStatus),
            SubmittedAt = row.request.CreatedAt ,
            CanCancel = ActiveStatuses.Contains(row.request.RequestStatus),
            Trip = new TripDetailPayload
            {
                EmployeeName = row.employee?.EmployeeName,
                DeptName = row.employee?.DeptCode,
                TripCode = row.request.TripCode,
                StartDate = row.request.StartDate,
                EndDate = row.request.EndDate,
                Destination = row.request.Destination,
                Purpose = row.request.Purpose,
                CustomerOrPartner = row.request.CustomerOrPartner,
                TransportMethod = row.request.TransportMethod,
                CompanionEmployeeCodes = row.request.CompanionEmployeeCodes,
                EstimatedCost = row.request.EstimatedCost,
                Accommodation = row.request.Accommodation,
                Note = row.request.Note
            },
            ApprovalSteps = await GetApprovalStepsAsync(id, ct)
        });
    }

    public override Task<ServiceResult<BalanceSummaryDto>> GetBalanceAsync(
        int year, UserIdentityDto user, CancellationToken ct)
        => Task.FromResult(ServiceResult<BalanceSummaryDto>.Ok(new BalanceSummaryDto
        {
            Kind = RequestModule.Trip,
            Year = year,
            Entitled = 0,
            Used = 0,
            Remaining = 0,
            PendingCount = 0
        }));

    private static string GetStatusColor(ApprovalStatus status) => status switch
    {
        ApprovalStatus.Approved => "success",
        ApprovalStatus.Rejected => "error",
        ApprovalStatus.Cancelled => "default",
        ApprovalStatus.InProgress or ApprovalStatus.Escalated => "info",
        _ => "warning"
    };
}