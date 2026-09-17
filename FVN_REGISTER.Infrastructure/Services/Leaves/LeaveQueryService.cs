using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Application.Interfaces.Histories;
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Maps;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Entities.Leaves;
using FVN_REGISTER.Core.Entities.Views;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Core.Utils;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Leaves
{
    public class LeaveQueryService
        : BaseRequestQueryService<LeaveSummaryDto, LeaveBalanceDto, LeaveRequestDetailDto, LeaveRequestDto>,
          ILeaveQueryService
    {
        private readonly IApprovalProvider<LeaveApprovalSubject> _approvalProvider;

        protected override RequestModule Module => RequestModule.Leave;

        public LeaveQueryService(
            IUnitOfWork uow,
            IApprovalHistoryService historyService,
            IAttachmentService attachmentService,
            ICurrentUserService currentUserService,
            IApprovalProvider<LeaveApprovalSubject> approvalProvider)
            : base(uow, historyService, attachmentService, currentUserService)
        {
            _approvalProvider = approvalProvider;
        }

        protected override async Task<LeaveRequestDto?> GetHeaderByIdAsync(int requestId, CancellationToken ct)
        {
            var entity = await Uow.Repository<F03LeaveDay>().Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == requestId && x.IsActive == true, ct);

            if (entity == null) return null;

            var requester = await Uow.Repository<VF03employee>().Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeCode == entity.EmployeeCode, ct);

            F03Department? department = null;
            if (!string.IsNullOrEmpty(entity.DeptCode))
            {
                department = await Uow.Repository<F03Department>().Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.DeptCode == entity.DeptCode, ct);
            }

            return LeaveMapper.ToDto(entity, requester, department);
        }

        protected override async Task<List<LeaveRequestDetailDto>> GetDetailsByIdAsync(int requestId, CancellationToken ct)
        {
            var details = await Uow.Repository<F03LeaveDayDetail>().Query()
                .AsNoTracking()
                .Where(d => d.LeaveDaysId == requestId)
                .ToListAsync(ct);

            return details.Select(LeaveMapper.ToDetailDto).ToList();
        }

        public override async Task<List<WidgetCounterDto>> GetMyWidgetsAsync(
            string employeeCode, CancellationToken ct = default)
        {
            var pendingCount = await Uow.Repository<VF03LeaveRequest>().Query()
                .AsNoTracking()
                .CountAsync(x => x.EmployeeCode == employeeCode
                               && x.IsActive == true
                               && (x.RequestStatus == ApprovalStatus.Pending || x.RequestStatus == ApprovalStatus.InProgress), ct);

            var usedThisYear = await Uow.Repository<VF03LeaveRequest>().Query()
                .AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode
                         && x.IsActive == true
                         && x.RequestStatus == ApprovalStatus.Approved
                         && x.WorkYear == DateTime.Now.Year)
                .SumAsync(x => (decimal?)x.TotalLeaveDay, ct) ?? 0;

            return new List<WidgetCounterDto>
            {
                new() { Title = "Đơn nghỉ chờ duyệt", Value = pendingCount.ToString(), Icon = "PendingActions", Color = "Warning", Link = "/leave/history" },
                new() { Title = "Ngày phép đã dùng", Value = usedThisYear.ToString("0.#"), Icon = "EventBusy", Color = "Info", Link = "/leave/history" }
            };
        }

        public override async Task<List<LeaveSummaryDto>> GetRecentSummaryAsync(
            string employeeCode, int limit = 5, CancellationToken ct = default)
        {
            var data = await Uow.Repository<VF03LeaveRequest>().Query()
                .AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode && x.IsActive == true)
                .OrderByDescending(x => x.CreatedAt)
                .Take(limit)
                .ToListAsync(ct);

            return data.Select(MapToSummary).ToList();
        }

        public override async Task<LeaveBalanceDto> GetSimpleBalanceAsync(
            string employeeCode, int year, CancellationToken ct = default)
        {
            var phep = await Uow.Repository<VF03LeaveBalance>().Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.EmployeeCode == employeeCode && x.WorkYear == year, ct);

            var pendingDays = await Uow.Repository<F03LeaveDay>().Query()
                .AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode
                         && x.WorkYear == year
                         && x.IsActive == true
                         && (x.RequestStatus == ApprovalStatus.Pending || x.RequestStatus == ApprovalStatus.InProgress))
                .SumAsync(x => (decimal?)x.TotalDay, ct) ?? 0;

            var sickCodes = new[] { "0013", "0014", "0033" };

            var approvedDetails = await Uow.Repository<F03LeaveDayDetail>().Query()
                .AsNoTracking()
                .Where(d => d.LeaveDay.EmployeeCode == employeeCode
                         && d.LeaveDay.WorkYear == year
                         && d.LeaveDay.IsActive == true
                         && d.LeaveDay.RequestStatus == ApprovalStatus.Approved)
                .Select(d => new { d.LeaveTypeCode, d.IsCountedAsLeave, d.DayValue })
                .ToListAsync(ct);

            var sick = approvedDetails
                .Where(d => sickCodes.Contains(d.LeaveTypeCode))
                .Sum(d => d.DayValue);

            var unpaid = approvedDetails
                .Where(d => !d.IsCountedAsLeave && !sickCodes.Contains(d.LeaveTypeCode))
                .Sum(d => d.DayValue);

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
                Sick = sick
            };
        }

        public override async Task<PaginationResult<LeaveSummaryDto>> GetPagedAsync(
            string? deptCode, ApprovalStatus? status, DateTime? fromDate, DateTime? toDate,
            int page, int pageSize, CancellationToken ct = default)
        {
            var query = Uow.Repository<VF03LeaveRequest>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true);

            if (!string.IsNullOrWhiteSpace(deptCode))
                query = query.Where(x => x.DeptCode == deptCode);
            if (status.HasValue)
                query = query.Where(x => x.RequestStatus == status.Value);
            if (fromDate.HasValue)
                query = query.Where(x => x.StartDate >= fromDate.Value.Date);
            if (toDate.HasValue)
                query = query.Where(x => x.EndDate <= toDate.Value.Date);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.RegisterDate)
                .ThenByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var dtos = items.Select(MapToSummary).ToList();
            return new PaginationResult<LeaveSummaryDto>(dtos, totalCount, page, pageSize);
        }

        public override async Task<List<LeaveRequestDto>> GetDeptByDateAsync(
            string deptCode, DateTime date, CancellationToken ct = default)
        {
            var data = await Uow.Repository<VF03LeaveRequest>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true
                         && x.DeptCode == deptCode
                         && x.StartDate <= date.Date
                         && x.EndDate >= date.Date
                         && x.RequestStatus != ApprovalStatus.Cancelled
                         && x.RequestStatus != ApprovalStatus.Rejected)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);

            return data.Select(LeaveMapper.ToDtoFromView).ToList();
        }

        public async Task<SystemMasterDataDto> GetCombinedDataAsync(
            string empCode, string deptCode, string positionCode, int year, CancellationToken ct = default)
        {
            var holidays = await Uow.Repository<F03CompanyHoliday>().Query()
                .AsNoTracking()
                .Where(h => h.Year == year)
                .ToListAsync(ct);

            var holidaysNotCount = holidays
                .Where(h => !h.IsPaidLeave)
                .Select(h => h.HolidayDate.ToString("yyyy-MM-dd"))
                .ToList();

            var calendarEvents = holidays.Select(h => new CalendarEventDto
            {
                Title = h.Description,
                Start = h.HolidayDate,
                End = h.HolidayDate,
                Module = RequestModule.Leave,
                Status = ApprovalStatus.Approved
            }).ToList();

            var workYears = await Uow.Repository<F03WorkYear>().Query()
                .AsNoTracking()
                .OrderByDescending(x => x.WorkYear)
                .Select(x => new WorkYearDto
                {
                    Id = x.Id,
                    Year = x.WorkYear,
                    YearName = $"Năm {x.WorkYear}",
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    IsActive = x.IsActive == true,
                })
                .ToListAsync(ct);

            var leaveTypes = await Uow.Repository<F03LeaveType>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true)
                .OrderBy(x => x.LeaveTypeCode)
                .Select(x => new FVN_REGISTER.Contract.Dtos.LeaveTypes.LeaveTypeDto
                {
                    Id = x.Id,
                    LeaveTypeCode = x.LeaveTypeCode,
                    LeaveTypeName = x.LeaveTypeName,
                    LeaveTypeName2 = x.LeaveTypeName2,
                    IsCountedAsLeave = x.IsCountedAsLeave,
                    HRMCode = x.HRMCode,
                    IsActive = x.IsActive
                })
                .ToListAsync(ct);

            var leaveRequests = await Uow.Repository<VF03LeaveRequest>().Query()
                .AsNoTracking()
                .Where(x => x.EmployeeCode == empCode
                         && x.IsActive == true
                         && x.WorkYear == year)
                .OrderBy(x => x.StartDate)
                .ToListAsync(ct);

            var leaveEvents = leaveRequests.Select(x => new LeaveCalendarEventDto
            {
                RequestId = x.Id,
                LeaveCode = x.LeaveCode ?? string.Empty,
                EmployeeCode = x.EmployeeCode,
                RegisterDate = x.RegisterDate,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                TotalDay = x.TotalDay,
                TotalLeaveDay = x.TotalLeaveDay ?? 0,
                LeaveTypeCode = x.LeaveTypeCode ?? string.Empty,
                LeaveTypeName = x.LeaveTypeName ?? string.Empty,
                Reason = x.LeaveReason ?? string.Empty,
                Status = x.RequestStatus.ToString()
            }).ToList();

            var ctx = ApprovalBuildContext.ForLeave(empCode, deptCode, positionCode, year);
            var snapshotSteps = await _approvalProvider.BuildHierarchyAsync(ctx, ct);

            var defaultFlow = snapshotSteps.Select(s => new ApprovalStepDto
            {
                Level = s.Level,
                RoleName = s.RoleName,
                ApproverCode = s.ApproverCode,
                ApproverName = s.ApproverName,
                ApproverEmail = s.ApproverEmail,
                IsRequired = s.IsRequired
            }).ToList();

            return new SystemMasterDataDto
            {
                CompanyHolidays = calendarEvents,
                HolidaysNotCountLeave = holidaysNotCount,
                FiscalYears = workYears,
                LeaveTypes = leaveTypes,
                LeaveEvents = leaveEvents,
                DefaultApprovalFlow = defaultFlow
            };
        }

        private static LeaveSummaryDto MapToSummary(VF03LeaveRequest x) => new()
        {
            Id = x.Id,
            RequestCode = x.LeaveCode ?? "",
            RequestType = RequestModule.Leave,
            TypeName = x.LeaveTypeName ?? "Nghỉ phép",
            Status = x.RequestStatus,
            CreatedAt = x.CreatedAt ?? DateTime.Now,
            EmployeeName = x.EmployeeName,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            TotalDays = x.TotalDay
        };
    }
}