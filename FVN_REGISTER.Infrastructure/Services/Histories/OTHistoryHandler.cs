using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Extensions;
using FVN_REGISTER.Infrastructure.Services.Histories;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Histories;

public class OTHistoryHandler : BaseHistoryHandler<F03OTRequest>
{
    protected override RequestModule ModuleKind => RequestModule.Overtime;

    public OTHistoryHandler(FVNWEBAPPContext db) : base(db) { }

    protected override System.Linq.Expressions.Expression<Func<F03OTRequest, bool>> BuildIdPredicate(int id)
        => x => x.Id == id;

    public override async Task<ServiceResult<PaginationResult<HistoryItemDto>>> GetHistoryAsync(
        HistoryFilterDto filter, UserIdentityDto user, CancellationToken ct)
    {
        var q = Db.VF03OTRequests
            .AsNoTracking()
            .Where(x => x.IsActive == true && x.EmployeeCode == user.EmployeeCode);

        if (filter.Year.HasValue)
            q = q.Where(x => x.OTDate.Year == filter.Year.Value);

        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<ApprovalStatus>(filter.Status, true, out var status))
        {
            q = q.Where(x => x.RequestStatus == status);
        }

        if (filter.FromDate.HasValue)
            q = q.Where(x => x.OTDate >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            q = q.Where(x => x.OTDate <= filter.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var s = filter.SearchText.ToLower();
            q = q.Where(x =>
                (x.OTReasonSummary != null && x.OTReasonSummary.ToLower().Contains(s)) ||
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

        var ids = data.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToList();
        var stepsMap = await GetApprovalStepsMapAsync(ids, ct);

        var items = data.Select(x =>
        {
            var id = x.Id ?? 0;
            var statusValue = x.RequestStatus;
            var totalHours = x.TotalOTHours ?? 0;

            return new HistoryItemDto
            {
                Id = id,
                Kind = RequestModule.Overtime,
                EmployeeCode = x.EmployeeCode ?? string.Empty,
                EmployeeName = x.EmployeeName ?? string.Empty,
                DeptCode = x.DeptCode,
                SubmittedAt = x.CreatedAt ?? DateTime.Now,
                RequestStatus = statusValue.ToString(),
                StatusDisplay = statusValue.ToDisplayName(),
                StatusColor = GetStatusColor(statusValue),
                Reason = x.OTReasonSummary,
                StartDate = x.OTDate,
                EndDate = x.OTDate,
                OTDate = DateOnly.FromDateTime(x.OTDate),
                TotalDay = totalHours,
                TotalOTHours = totalHours,
                OTTypeName = x.OTTypeName,
                EmployeeCount = x.EmployeeCount,
                CanCancel = ActiveStatuses.Contains(statusValue),
                ApprovalSteps = stepsMap.GetValueOrDefault(id, new())
            };
        }).ToList();

        return ServiceResult<PaginationResult<HistoryItemDto>>.Ok(
            new PaginationResult<HistoryItemDto>(items, total, filter.Page, filter.PageSize));
    }

    public override async Task<ServiceResult<BalanceSummaryDto>> GetBalanceAsync(
        int year, UserIdentityDto user, CancellationToken ct)
    {
        var validStatuses = new[]
        {
            ApprovalStatus.Draft,
            ApprovalStatus.Pending,
            ApprovalStatus.InProgress,
            ApprovalStatus.Escalated,
            ApprovalStatus.NeedsRevision,
            ApprovalStatus.Approved
        };

        var totalHours = await Db.OvertimeEmployees
            .AsNoTracking()
            .Where(e => e.EmployeeCode == user.EmployeeCode
                     && e.OTRequest.IsActive == true
                     && validStatuses.Contains(e.OTRequest.RequestStatus)
                     && e.OTRequest.OTDate.Year == year)
            .SumAsync(e => (decimal?)e.OTHours, ct) ?? 0;

        var pendingCount = await Db.OvertimeRequests
            .AsNoTracking()
            .CountAsync(x =>
                x.EmployeeCode == user.EmployeeCode &&
                x.OTDate.Year == year &&
                x.IsActive == true &&
                ActiveStatuses.Contains(x.RequestStatus), ct);

        return ServiceResult<BalanceSummaryDto>.Ok(new BalanceSummaryDto
        {
            Kind = RequestModule.Overtime,
            Year = year,
            Entitled = 0,
            Used = totalHours,
            Remaining = 0,
            PendingCount = pendingCount
        });
    }

    public override Task<ServiceResult<HistoryItemDetailDto>> GetDetailAsync(
        int id, UserIdentityDto user, CancellationToken ct)
        => throw new NotImplementedException("Cần implement GetDetailAsync cho OT.");

    private static string GetStatusColor(ApprovalStatus status) => status switch
    {
        ApprovalStatus.Approved => "success",
        ApprovalStatus.Rejected => "error",
        ApprovalStatus.Cancelled => "default",
        ApprovalStatus.InProgress or ApprovalStatus.Escalated => "info",
        _ => "warning"
    };
}
