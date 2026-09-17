using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Contract.Interfaces.Histories;
using FVN_REGISTER.Contract.Models.Data;
using FVN_REGISTER.Contract.Models.Entities.Leaves;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Contract.ViewModels.OT;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FVN_REGISTER.API.Services.Histories
{
    public class LeaveHistoryHandler : BaseHistoryHandler<F03leaveDay>
    {
        public LeaveHistoryHandler(FVNWEBAPPContext db) : base(db) { }

        public override string Kind => RequestTypeDf.Leave;

        protected override string[] ActiveStatuses => LeaveStatus.ActiveStatuses;

        protected override string CancelledStatus => LeaveStatus.Cancel;

        protected override Expression<Func<F03leaveDay, bool>> BuildIdPredicate(int id)
            => x => x.Id == id;

        public override async Task<ServiceResult<PaginationResult<HistoryItemDto>>> GetHistoryAsync(
            HistoryFilterDto filter, UserIdentityDto user, CancellationToken ct)
        {
            var q = Db.VF03LeaveRequests
                .AsNoTracking()
                .Where(x => x.IsActive == true && x.EmployeeCode == user.EmployeeCode);

            if (filter.Year.HasValue)
                q = q.Where(x => x.StartDate.Year == filter.Year);

            if (!string.IsNullOrEmpty(filter.Status))
                q = q.Where(x => x.RequestStatus == filter.Status);

            if (filter.FromDate.HasValue)
                q = q.Where(x => x.StartDate >= filter.FromDate);

            if (filter.ToDate.HasValue)
                q = q.Where(x => x.EndDate <= filter.ToDate);

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

            var ids = data.Select(x => x.LeaveId).ToList();
            var stepsMap = await GetApprovalStepsMapAsync(ids, ct);

            var items = data.Select(x => new HistoryItemDto
            {
                Id = x.LeaveId,
                Kind = RequestTypeDf.Leave,
                EmployeeCode = x.EmployeeCode ?? "",
                EmployeeName = x.EmployeeName,
                DeptCode = x.DeptCode,
                SubmittedAt = x.CreatedAt ?? DateTime.Now,
                RequestStatus = x.RequestStatus ?? LeaveStatus.Pending,
                StatusDisplay = LeaveStatus.GetDisplayName(x.RequestStatus ?? ""),
                StatusColor = LeaveStatus.GetColor(x.RequestStatus ?? ""),
                Reason = x.LeaveReason,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                TotalDay = x.TotalDay,
                TotalLeaveDay = x.TotalLeaveDay ?? 0,
                LeaveTypeName = x.LeaveTypeName,
                CanCancel = LeaveStatus.ActiveStatuses.Contains(x.RequestStatus ?? ""),
                ApprovalSteps = stepsMap.GetValueOrDefault(x.LeaveId, new())
            }).ToList();

            return ServiceResult<PaginationResult<HistoryItemDto>>.Ok(
                new PaginationResult<HistoryItemDto>(items, total, filter.Page, filter.PageSize));
        }

        public override async Task<ServiceResult<BalanceSummaryDto>> GetBalanceAsync(
            int year, UserIdentityDto user, CancellationToken ct)
        {
            var phep = await Db.VF03phepTons
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.EmployeeCode == user.EmployeeCode && x.WorkYear == year, ct);

            var pendingCount = await Db.F03leaveDays
                .AsNoTracking()
                .CountAsync(x =>
                    x.EmployeeCode == user.EmployeeCode &&
                    x.WorkYear == year &&
                    x.IsActive == true &&
                    LeaveStatus.ActiveStatuses.Contains(x.RequestStatus), ct);

            return ServiceResult<BalanceSummaryDto>.Ok(new BalanceSummaryDto
            {
                Kind = RequestTypeDf.Leave,
                Year = year,
                Entitled = phep?.TongPhep ?? 0,
                Used = phep?.SoNgayNghiPhep ?? 0,
                Remaining = phep?.PhepTon ?? 0,
                PendingCount = pendingCount
            });
        }

        public override Task<ServiceResult<HistoryItemDetailDto>> GetDetailAsync(
            int id, UserIdentityDto user, CancellationToken ct)
            => throw new NotImplementedException("Cần implement GetDetailAsync cho Leave.");

        // ❌ CancelAsync KHÔNG còn override — dùng nguyên bản chung ở BaseHistoryHandler
    }
}
