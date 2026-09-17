using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Extensions;
using FVN_REGISTER.Infrastructure.Services.Histories;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Histories;

public class LeaveHistoryHandler : BaseHistoryHandler<F03LeaveDay>
{
    protected override RequestModule ModuleKind => RequestModule.Leave;

    public LeaveHistoryHandler(FVNWEBAPPContext db) : base(db) { }

    protected override System.Linq.Expressions.Expression<Func<F03LeaveDay, bool>> BuildIdPredicate(int id)
        => x => x.Id == id;

    public override async Task<ServiceResult<PaginationResult<HistoryItemDto>>> GetHistoryAsync(
        HistoryFilterDto filter, UserIdentityDto user, CancellationToken ct)
    {
        var q = Db.VF03LeaveRequests
            .AsNoTracking()
            .Where(x => x.IsActive == true && x.EmployeeCode == user.EmployeeCode);

        if (filter.Year.HasValue)
            q = q.Where(x => x.StartDate.Year == filter.Year.Value);

        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<ApprovalStatus>(filter.Status, true, out var status))
        {
            q = q.Where(x => x.RequestStatus == status);
        }

        if (filter.FromDate.HasValue)
            q = q.Where(x => x.StartDate >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            q = q.Where(x => x.EndDate <= filter.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var s = filter.SearchText.ToLower();
            q = q.Where(x =>
                (x.LeaveReason != null && x.LeaveReason.ToLower().Contains(s)) ||
                (x.EmployeeName != null && x.EmployeeName.ToLower().Contains(s)));
        }

        var total = await q.CountAsync(ct);
        var data = await q
            .OrderByDescending(x => x.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        if (data.Count == 0)
            return ServiceResult<PaginationResult<HistoryItemDto>>.Ok(
                new PaginationResult<HistoryItemDto>(new(), total, filter.Page, filter.PageSize));

        var ids = data.Select(x => x.Id).ToList();
        var stepsMap = await GetApprovalStepsMapAsync(ids, ct);

        var items = data.Select(x => new HistoryItemDto
        {
            Id = x.Id,
            Kind = RequestModule.Leave,
            EmployeeCode = x.EmployeeCode ?? string.Empty,
            EmployeeName = x.EmployeeName,
            DeptCode = x.DeptCode,
            SubmittedAt = x.CreatedAt ?? DateTime.Now,
            RequestStatus = x.RequestStatus.ToString(),
            StatusDisplay = x.RequestStatus.ToDisplayName(),
            StatusColor = GetStatusColor(x.RequestStatus),
            Reason = x.LeaveReason,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            TotalDay = x.TotalDay,
            TotalLeaveDay = x.TotalLeaveDay ?? 0,
            LeaveTypeName = x.LeaveTypeName,
            CanCancel = ActiveStatuses.Contains(x.RequestStatus),
            ApprovalSteps = stepsMap.GetValueOrDefault(x.Id, new())
        }).ToList();

        return ServiceResult<PaginationResult<HistoryItemDto>>.Ok(
            new PaginationResult<HistoryItemDto>(items, total, filter.Page, filter.PageSize));
    }

    public override async Task<ServiceResult<BalanceSummaryDto>> GetBalanceAsync(
        int year, UserIdentityDto user, CancellationToken ct)
    {
        var balance = await Db.VF03LeaveBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.EmployeeCode == user.EmployeeCode && x.WorkYear == year, ct);

        var pendingCount = await Db.LeaveDays
            .AsNoTracking()
            .CountAsync(x =>
                x.EmployeeCode == user.EmployeeCode &&
                x.WorkYear == year &&
                x.IsActive == true &&
                ActiveStatuses.Contains(x.RequestStatus), ct);

        return ServiceResult<BalanceSummaryDto>.Ok(new BalanceSummaryDto
        {
            Kind = RequestModule.Leave,
            Year = year,
            Entitled = balance?.TotalEntitledLeave ?? 0,
            Used = balance?.LeaveDaysUsed ?? 0,
            Remaining = balance?.RemainingLeave ?? 0,
            PendingCount = pendingCount
        });
    }

    public override Task<ServiceResult<HistoryItemDetailDto>> GetDetailAsync(
        int id, UserIdentityDto user, CancellationToken ct)
        => throw new NotImplementedException("Cần implement GetDetailAsync cho Leave.");

    private static string GetStatusColor(ApprovalStatus status) => status switch
    {
        ApprovalStatus.Approved => "success",
        ApprovalStatus.Rejected => "error",
        ApprovalStatus.Cancelled => "default",
        ApprovalStatus.InProgress or ApprovalStatus.Escalated => "info",
        _ => "warning"
    };
}
