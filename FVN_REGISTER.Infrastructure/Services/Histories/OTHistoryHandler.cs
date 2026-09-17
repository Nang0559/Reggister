
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Core.Entities.OT;
using FVN_REGISTER.Core.Utils;
using FVN_REGISTER.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FVN_REGISTER.API.Services.Histories
{
    public class OTHistoryHandler : BaseHistoryHandler<F03OTRequest>
    {
        public OTHistoryHandler(FVNWEBAPPContext db) : base(db) { }

        public override string Kind => RequestTypeDf.OT;

        protected override string[] ActiveStatuses => OTStatus.ActiveStatuses;

        protected override string CancelledStatus => OTStatus.Cancelled;

        protected override Expression<Func<F03OTRequest, bool>> BuildIdPredicate(int id)
            => x => x.Id == id;

        public override async Task<ServiceResult<PaginationResult<HistoryItemDto>>> GetHistoryAsync(
            HistoryFilterDto filter, UserIdentityDto user, CancellationToken ct)
        {
            var q = Db.VF03OTRequests
                .AsNoTracking()
                .Where(x => x.IsActive == true && x.EmployeeCode == user.EmployeeCode);

            if (filter.Year.HasValue)
                q = q.Where(x => x.OTDate.Year == filter.Year);

            if (!string.IsNullOrEmpty(filter.Status))
                q = q.Where(x => x.RequestStatus == filter.Status);

            if (filter.FromDate.HasValue)
                q = q.Where(x => x.OTDate >= DateOnly.FromDateTime(filter.FromDate.Value));

            if (filter.ToDate.HasValue)
                q = q.Where(x => x.OTDate <= DateOnly.FromDateTime(filter.ToDate.Value));

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

            var items = data.Select(x => new HistoryItemDto
            {
                Id = x.Id ?? 0,
                Kind = RequestTypeDf.OT,
                EmployeeCode = x.EmployeeCode ?? "",
                EmployeeName = x.EmployeeName,
                DeptCode = x.DeptCode,
                SubmittedAt = x.CreatedAt ?? DateTime.Now,
                RequestStatus = x.RequestStatus ?? OTStatus.Pending,
                StatusDisplay = OTStatus.GetDisplayName(x.RequestStatus ?? ""),
                StatusColor = OTStatus.GetColor(x.RequestStatus ?? ""),
                Reason = x.OTReasonSummary,
                StartDate = x.StartTime,
                EndDate = x.EndTime,
                TotalDay = x.TotalOTHours ?? 0,
                TotalLeaveDay = 0,
                LeaveTypeName = x.OTTypeName,
                CanCancel = OTStatus.ActiveStatuses.Contains(x.RequestStatus ?? ""),
                ApprovalSteps = x.Id.HasValue
                    ? stepsMap.GetValueOrDefault(x.Id.Value, new())
                    : new()
            }).ToList();

            return ServiceResult<PaginationResult<HistoryItemDto>>.Ok(
                new PaginationResult<HistoryItemDto>(items, total, filter.Page, filter.PageSize));
        }

        public override async Task<ServiceResult<BalanceSummaryDto>> GetBalanceAsync(
            int year, UserIdentityDto user, CancellationToken ct)
        {
            var validStatuses = OTStatus.ActiveStatuses
                .Concat(new[] { OTStatus.Approved })
                .ToArray();

            var totalHours = await Db.F03OTEmployees
                .AsNoTracking()
                .Where(e => e.EmployeeCode == user.EmployeeCode
                         && e.OTRequest.IsActive == true
                         && validStatuses.Contains(e.OTRequest.RequestStatus)
                         && e.OTRequest.OTDate.Year == year)
                .SumAsync(e => e.OTHours, ct);

            var pendingCount = await Db.F03OTRequests
                .AsNoTracking()
                .CountAsync(x =>
                    x.EmployeeCode == user.EmployeeCode &&
                    x.OTDate.Year == year &&
                    x.IsActive == true &&
                    OTStatus.ActiveStatuses.Contains(x.RequestStatus), ct);

            return ServiceResult<BalanceSummaryDto>.Ok(new BalanceSummaryDto
            {
                Kind = RequestTypeDf.OT,
                Year = year,
                Entitled = 0,              // OT không có "định mức được cấp" như Leave
                Used = totalHours,
                Remaining = 0,             // hoặc tính theo YearlyLimit nếu cần
                PendingCount = pendingCount
            });
        }

        public override Task<ServiceResult<HistoryItemDetailDto>> GetDetailAsync(
            int id, UserIdentityDto user, CancellationToken ct)
            => throw new NotImplementedException("Cần implement GetDetailAsync cho OT.");

        // ❌ CancelAsync KHÔNG override — dùng chung ở BaseHistoryHandler
    }
}
