using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Application.Interfaces.Histories;
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Application.Maps;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Infrastructure.Services.Common;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Leaves
{
    public class LeaveQueryService
        : BaseRequestQueryService<LeaveSummaryDto, LeaveBalanceDto, LeaveRequestDetailDto, LeaveRequestDto>,
          ILeaveQueryService
    {
        private readonly IApprovalProvider<LeaveRequestSubject> _approvalProvider;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuthorizationService _authorization;
        private readonly ILeaveEntitlementService _leaveEntitlement;
        protected override RequestModule Module => RequestModule.Leave;

        public LeaveQueryService(
            IUnitOfWork uow,
            IApprovalHistoryService historyService,
            IAttachmentService attachmentService,
            ICurrentUserService currentUserService,
            IApprovalProvider<LeaveRequestSubject> approvalProvider,
            IAuthorizationService authorization,
            ILeaveEntitlementService leaveEntitlement)
            : base(uow, historyService, attachmentService, currentUserService)
        {
            _approvalProvider = approvalProvider;
            _currentUser = currentUserService;
            _authorization = authorization;
            _leaveEntitlement = leaveEntitlement;
        }

        protected override async Task<LeaveRequestDto?> GetHeaderByIdAsync(int requestId, CancellationToken ct)
        {
            var entity = await Uow.Repository<F03LeaveDay>().Query().AsNoTracking().FirstOrDefaultAsync(x => x.Id == requestId && x.IsActive == true, ct);
            if (entity == null) return null;
            var user = _currentUser.GetCurrentUser();
            if (user == null || !await _authorization.CanAccessAsync(user, SecurityFunctionCodes.LeaveView, entity.EmployeeCode, entity.DeptCode, ct))
                return null;
            var requester = await Uow.Repository<VF03employee>().Query().AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeCode == entity.EmployeeCode, ct);
            F03Department? department = null;
            if (!string.IsNullOrEmpty(entity.DeptCode))
                department = await Uow.Repository<F03Department>().Query().AsNoTracking().FirstOrDefaultAsync(d => d.DeptCode == entity.DeptCode, ct);
            return LeaveMapper.ToDto(entity, requester, department);
        }

        protected override async Task<List<LeaveRequestDetailDto>> GetDetailsByIdAsync(int requestId, CancellationToken ct)
        {
            var details = await Uow.Repository<F03LeaveDayDetail>().Query().AsNoTracking().Where(d => d.LeaveDaysId == requestId).ToListAsync(ct);
            return details.Select(LeaveMapper.ToDetailDto).ToList();
        }

        public override async Task<List<WidgetCounterDto>> GetMyWidgetsAsync(string employeeCode, CancellationToken ct = default)
        {
            var pendingCount = await Uow.Repository<VF03LeaveRequest>().Query().AsNoTracking().CountAsync(x => x.EmployeeCode == employeeCode && x.IsActive == true && (x.RequestStatus == ApprovalStatus.Pending || x.RequestStatus == ApprovalStatus.InProgress), ct);
            var usedThisYear = await Uow.Repository<VF03LeaveRequest>().Query().AsNoTracking().Where(x => x.EmployeeCode == employeeCode && x.IsActive == true && x.RequestStatus == ApprovalStatus.Approved && x.WorkYear == DateTime.Now.Year).SumAsync(x => (decimal?)x.TotalLeaveDay, ct) ?? 0;
            return new List<WidgetCounterDto>
            {
                new() { Title = "Đơn nghỉ chờ duyệt", Value = pendingCount.ToString(), Icon = "PendingActions", Color = "Warning", Link = "/leave/history" },
                new() { Title = "Ngày phép đã dùng", Value = usedThisYear.ToString("0.#"), Icon = "EventBusy", Color = "Info", Link = "/leave/history" }
            };
        }

        public override async Task<List<LeaveSummaryDto>> GetRecentSummaryAsync(string employeeCode, int limit = 5, CancellationToken ct = default)
        {
            var data = await Uow.Repository<VF03LeaveRequest>().Query().AsNoTracking().Where(x => x.EmployeeCode == employeeCode && x.IsActive == true).OrderByDescending(x => x.CreatedAt).Take(limit).ToListAsync(ct);
            return data.Select(MapToSummary).ToList();
        }

        public override async Task<LeaveBalanceDto> GetSimpleBalanceAsync(string employeeCode, int year, CancellationToken ct = default)
        {
            // Entitlement là dữ liệu được tính từ FirstWorkingDate + F03WorkYear.
            // Recalculate trước khi đọc view balance để tránh dùng snapshot cũ.
            await _leaveEntitlement.EnsureCalculatedAsync(employeeCode, year, ct);

            var phep = await Uow.Repository<VF03LeaveBalance>().Query().AsNoTracking().FirstOrDefaultAsync(x => x.EmployeeCode == employeeCode && x.WorkYear == year, ct);
            var pendingDays = await Uow.Repository<F03LeaveDay>().Query().AsNoTracking().Where(x => x.EmployeeCode == employeeCode && x.WorkYear == year && x.IsActive == true && (x.RequestStatus == ApprovalStatus.Pending || x.RequestStatus == ApprovalStatus.InProgress)).SumAsync(x => (decimal?)x.TotalDay, ct) ?? 0;
            var sickCodes = new[] { "0013", "0014", "0033" };

            // Do not traverse F03LeaveDay from the detail query here.
            // The current database does not expose the legacy LeaveCode column
            // that exists on the EF entity. A navigation-based query causes
            // EF Core to materialize the whole F03LeaveDay row and therefore
            // selects LeaveCode, breaking Dashboard/Leave balance queries.
            // Project the required request ids first, then query only detail
            // columns. This keeps the read path aligned with the current SQL
            // schema and avoids pulling unmapped legacy columns.
            var approvedLeaveIds = await Uow.Repository<F03LeaveDay>().Query()
                .AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode
                    && x.WorkYear == year
                    && x.IsActive == true
                    && x.RequestStatus == ApprovalStatus.Approved)
                .Select(x => x.Id)
                .ToListAsync(ct);

            var approvedDetails = approvedLeaveIds.Count == 0
                ? new List<(string? LeaveTypeCode, bool IsCountedAsLeave, decimal DayValue)>()
                : await Uow.Repository<F03LeaveDayDetail>().Query()
                    .AsNoTracking()
                    .Where(d => approvedLeaveIds.Contains(d.LeaveDaysId))
                    .Select(d => new { d.LeaveTypeCode, d.IsCountedAsLeave, d.DayValue })
                    .AsEnumerable()
                    .Select(d => (d.LeaveTypeCode, d.IsCountedAsLeave, d.DayValue))
                    .ToList();

            var sick = approvedDetails.Where(d => sickCodes.Contains(d.LeaveTypeCode)).Sum(d => d.DayValue);
            var unpaid = approvedDetails.Where(d => !d.IsCountedAsLeave && !sickCodes.Contains(d.LeaveTypeCode)).Sum(d => d.DayValue);

            var today = DateTime.Today;
            var upcoming = await Uow.Repository<VF03LeaveRequest>().Query()
                .AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode
                    && x.IsActive == true
                    && x.RequestStatus == ApprovalStatus.Approved
                    && x.EndDate >= today
                    && x.WorkYear == year)
                .OrderBy(x => x.StartDate)
                .ToListAsync(ct);

            var upcomingDays = upcoming.Sum(x => x.TotalLeaveDay ?? x.TotalDay);
            var nextLeave = upcoming.FirstOrDefault();

            return new LeaveBalanceDto
            {
                EmployeeCode = employeeCode,
                EmployeeName = phep?.EmployeeName ?? string.Empty,
                WorkYear = year,
                Year = year,
                TotalEntitled = phep?.TotalEntitledLeave ?? 0,
                Used = phep?.LeaveDaysUsed ?? 0,
                Remaining = phep?.RemainingLeave ?? 0,
                Unpaid = unpaid,
                Sick = sick,
                UpcomingDays = upcomingDays,
                NextLeaveDate = nextLeave?.StartDate,
                NextLeaveTypeName = nextLeave?.LeaveTypeName
            };
        }

        public override async Task<PaginationResult<LeaveSummaryDto>> GetPagedAsync(string? deptCode, ApprovalStatus? status, DateTime? fromDate, DateTime? toDate, int page, int pageSize, CancellationToken ct = default)
        {
            var query = Uow.Repository<VF03LeaveRequest>().Query().AsNoTracking().Where(x => x.IsActive == true);
            var user = _currentUser.GetCurrentUser();
            if (user == null) return new PaginationResult<LeaveSummaryDto>(new List<LeaveSummaryDto>(), 0, page, pageSize);
            var scope = await _authorization.GetScopeAsync(user.UserId, SecurityFunctionCodes.LeaveView, ct);
            if (scope == AuthorizationScopeCodes.Own || scope == AuthorizationScopeCodes.Employee)
                query = query.Where(x => x.EmployeeCode == user.EmployeeCode);
            else if (scope == AuthorizationScopeCodes.Department)
                query = query.Where(x => x.DeptCode == user.DeptCode);
            else if (scope != AuthorizationScopeCodes.All)
                query = query.Where(x => false);
            if (!string.IsNullOrWhiteSpace(deptCode) && scope == AuthorizationScopeCodes.All) query = query.Where(x => x.DeptCode == deptCode);
            if (status.HasValue) query = query.Where(x => x.RequestStatus == status.Value);
            if (fromDate.HasValue) query = query.Where(x => x.StartDate >= fromDate.Value.Date);
            if (toDate.HasValue) query = query.Where(x => x.EndDate <= toDate.Value.Date);
            var totalCount = await query.CountAsync(ct);
            var items = await query.OrderByDescending(x => x.RegisterDate).ThenByDescending(x => x.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            return new PaginationResult<LeaveSummaryDto>(items.Select(MapToSummary).ToList(), totalCount, page, pageSize);
        }

        public override async Task<List<LeaveRequestDto>> GetDeptByDateAsync(string deptCode, DateTime date, CancellationToken ct = default)
        {
            var user = _currentUser.GetCurrentUser();
            if (user == null) return new List<LeaveRequestDto>();
            var scope = await _authorization.GetScopeAsync(user.UserId, SecurityFunctionCodes.LeaveView, ct);
            var query = Uow.Repository<VF03LeaveRequest>().Query().AsNoTracking().Where(x => x.IsActive == true
                && (scope == AuthorizationScopeCodes.All
                    ? x.DeptCode == deptCode
                    : scope == AuthorizationScopeCodes.Department
                        ? x.DeptCode == user.DeptCode
                        : (scope == AuthorizationScopeCodes.Own || scope == AuthorizationScopeCodes.Employee)
                            ? x.EmployeeCode == user.EmployeeCode
                            : false)
                && x.DeptCode == deptCode && x.StartDate <= date.Date && x.EndDate >= date.Date && x.RequestStatus != ApprovalStatus.Cancelled && x.RequestStatus != ApprovalStatus.Rejected);
            var data = await query.OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
            return data.Select(LeaveMapper.ToDtoFromView).ToList();
        }

        public async Task<SystemMasterDataDto> GetCombinedDataAsync(string empCode, string deptCode, string positionCode, int year, CancellationToken ct = default)
        {
            var holidays = await Uow.Repository<F03CompanyHoliday>().Query().AsNoTracking().Where(h => h.Year == year).ToListAsync(ct);
            var holidaysNotCount = holidays.Where(h => !h.TinhPhep).Select(h => h.HolidayDate.ToString("yyyy-MM-dd")).ToList();
            var calendarEvents = holidays.Select(h => new CalendarEventDto { Title = h.Description, Start = h.HolidayDate, End = h.HolidayDate, Module = RequestModule.Leave, Status = ApprovalStatus.Approved }).ToList();
            var workYears = await Uow.Repository<F03WorkYear>().Query().AsNoTracking().OrderByDescending(x => x.WorkYear).Select(x => new WorkYearDto { Id = x.Id, Year = x.WorkYear, YearName = $"Năm {x.WorkYear}", StartDate = x.StartDate, EndDate = x.EndDate, IsActive = x.IsActive == true }).ToListAsync(ct);
            var leaveTypes = await Uow.Repository<F03LeaveType>().Query().AsNoTracking().Where(x => x.IsActive == true).OrderBy(x => x.LeaveTypeCode).Select(x => new FVN_REGISTER.Contract.Dtos.LeaveTypes.LeaveTypeDto { Id = x.Id, LeaveTypeCode = x.LeaveTypeCode, LeaveTypeName = x.LeaveTypeName, LeaveTypeName2 = x.LeaveTypeName2, IsCountedAsLeave = x.IsCountedAsLeave, HRMCode = x.HRMCode, IsActive = x.IsActive == true }).ToListAsync(ct);
            var leaveRequests = await Uow.Repository<VF03LeaveRequest>().Query().AsNoTracking().Where(x => x.EmployeeCode == empCode && x.IsActive == true && x.WorkYear == year).OrderBy(x => x.StartDate).ToListAsync(ct);
            var leaveEvents = leaveRequests.Select(x => new LeaveCalendarEventDto { RequestId = x.Id, LeaveCode = x.LeaveCode ?? string.Empty, EmployeeCode = x.EmployeeCode, RegisterDate = x.RegisterDate, StartDate = x.StartDate, EndDate = x.EndDate, TotalDay = x.TotalDay, TotalLeaveDay = x.TotalLeaveDay ?? 0, LeaveTypeCode = x.LeaveTypeCode ?? string.Empty, LeaveTypeName = x.LeaveTypeName ?? string.Empty, Reason = x.LeaveReason ?? string.Empty, Status = x.RequestStatus.ToString() }).ToList();
            var ctx = ApprovalBuildContext.ForLeave(0, empCode, deptCode, positionCode, year, null);
            // Calendar/master-data is read-only. Missing approval configuration
            // must not make the calendar endpoint fail with HTTP 500.
            // Submit/command flows remain strict and still validate the full route.
            var snapshotResult = await _approvalProvider.BuildHierarchyResultAsync(ctx, ct, strict: false);
            var snapshotSteps = snapshotResult.Data ?? new List<ApprovalStepSnapshotDto>();
            var defaultFlow = snapshotSteps.Select(s => new ApprovalStepDto { Level = s.Level, RoleName = s.RoleName, ApproverCode = s.ApproverCode, ApproverName = s.ApproverName, ApproverEmail = s.ApproverEmail, IsRequired = s.IsRequired }).ToList();
            return new SystemMasterDataDto { CompanyHolidays = calendarEvents, HolidaysNotCountLeave = holidaysNotCount, FiscalYears = workYears, LeaveTypes = leaveTypes, LeaveEvents = leaveEvents, DefaultApprovalFlow = defaultFlow };
        }

        private static LeaveSummaryDto MapToSummary(VF03LeaveRequest x) => new() { Id = x.Id, RequestCode = x.LeaveCode ?? "", RequestType = RequestModule.Leave, TypeName = x.LeaveTypeName ?? "Nghỉ phép", Status = x.RequestStatus, CreatedAt = x.CreatedAt ?? DateTime.Now, EmployeeName = x.EmployeeName, StartDate = x.StartDate, EndDate = x.EndDate, TotalDays = x.TotalDay };
    }
}
