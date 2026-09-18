using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Application.Interfaces.Histories;
using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Maps;
using FVN_REGISTER.Infrastructure.Services.Common;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.LimitRuleDtos;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Requests.OT;

using FVN_REGISTER.Core.Repositories;

using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.OT
{
    public class OTQueryService
        : BaseRequestQueryService<OTSummaryDto, OTBalanceDto, OTEmployeeDto, OTRequestDto>,
          IOTQueryService
    {
        private readonly IApprovalProvider<OTRequestSubject> _approvalProvider;

        protected override RequestModule Module => RequestModule.Overtime;

        public OTQueryService(
            IUnitOfWork uow,
            IApprovalHistoryService historyService,
            IAttachmentService attachmentService,
            ICurrentUserService currentUserService,
            IApprovalProvider<OTRequestSubject> approvalProvider)
            : base(uow, historyService, attachmentService, currentUserService)
        {
            _approvalProvider = approvalProvider;
        }

        protected override async Task<OTRequestDto?> GetHeaderByIdAsync(int requestId, CancellationToken ct)
        {
            var entity = await Uow.Repository<F03OTRequest>().Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == requestId && x.IsActive == true, ct);

            if (entity == null)
                return null;

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

            return OTMapper.ToDto(entity, requester, department);
        }

        protected override async Task<List<OTEmployeeDto>> GetDetailsByIdAsync(int requestId, CancellationToken ct)
        {
            var employees = await Uow.Repository<F03OTEmployee>().Query()
                .AsNoTracking()
                .Where(e => e.OTRequestId == requestId && e.IsActive == true)
                .ToListAsync(ct);

            return employees.Select(OTMapper.ToEmployeeDto).ToList();
        }

        public override async Task<List<WidgetCounterDto>> GetMyWidgetsAsync(
            string employeeCode, CancellationToken ct = default)
        {
            var pendingCount = await Uow.Repository<VF03OTRequest>().Query()
                .AsNoTracking()
                .CountAsync(x => x.EmployeeCode == employeeCode
                    && x.IsActive == true
                    && (x.RequestStatus == ApprovalStatus.Pending || x.RequestStatus == ApprovalStatus.InProgress), ct);

            var hoursThisMonth = await Uow.Repository<VF03OTRequest>().Query()
                .AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode
                    && x.IsActive == true
                    && x.RequestStatus == ApprovalStatus.Approved
                    && x.OTDate.Month == DateTime.Now.Month
                    && x.OTDate.Year == DateTime.Now.Year)
                .SumAsync(x => (decimal?)x.TotalOTHours, ct) ?? 0;

            return new List<WidgetCounterDto>
            {
                new() { Title = "Đơn OT chờ duyệt", Value = pendingCount.ToString(), Icon = "PendingActions", Color = "Warning", Link = "/ot/history" },
                new() { Title = "Giờ OT tháng này", Value = hoursThisMonth.ToString("0.#"), Icon = "AccessTime", Color = "Info", Link = "/ot/history" }
            };
        }

        public override async Task<List<OTSummaryDto>> GetRecentSummaryAsync(
            string employeeCode, int limit = 5, CancellationToken ct = default)
        {
            var data = await Uow.Repository<VF03OTRequest>().Query()
                .AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode && x.IsActive == true)
                .OrderByDescending(x => x.CreatedAt)
                .Take(limit)
                .ToListAsync(ct);

            return data.Select(MapToSummary).ToList();
        }

        public override async Task<OTBalanceDto> GetSimpleBalanceAsync(
            string employeeCode, int year, CancellationToken ct = default)
            => await GetBalanceAsync(employeeCode, year, DateTime.Now.Month, ct);

        public override async Task<PaginationResult<OTSummaryDto>> GetPagedAsync(
            string? deptCode, ApprovalStatus? status, DateTime? fromDate, DateTime? toDate,
            int page, int pageSize, CancellationToken ct = default)
        {
            var query = Uow.Repository<VF03OTRequest>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true);

            if (!string.IsNullOrWhiteSpace(deptCode))
                query = query.Where(x => x.DeptCode == deptCode);
            if (status.HasValue)
                query = query.Where(x => x.RequestStatus == status.Value);
            if (fromDate.HasValue)
                query = query.Where(x => x.OTDate >= fromDate.Value.Date);
            if (toDate.HasValue)
                query = query.Where(x => x.OTDate <= toDate.Value.Date);

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(x => x.OTDate)
                .ThenByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PaginationResult<OTSummaryDto>(
                items.Select(MapToSummary).ToList(), totalCount, page, pageSize);
        }

        public override async Task<List<OTRequestDto>> GetDeptByDateAsync(
            string deptCode, DateTime date, CancellationToken ct = default)
        {
            var data = await Uow.Repository<VF03OTRequest>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true
                    && x.DeptCode == deptCode
                    && x.OTDate.Date == date.Date
                    && x.RequestStatus != ApprovalStatus.Cancelled
                    && x.RequestStatus != ApprovalStatus.Rejected)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);

            return data.Select(x => OTMapper.ToDto(x)).ToList();
        }

        public async Task<OTCombinedDataDto> GetCombinedDataAsync(
            string employeeCode, string deptCode, int year, int month, CancellationToken ct = default)
        {
            var deptEmployees = await GetDeptEmployeesAsync(deptCode, ct);
            var balance = await GetBalanceAsync(employeeCode, year, month, ct);
            var recent = await GetRecentSummaryAsync(employeeCode, 5, ct);

            var limits = await Uow.Repository<F03OTLimitRule>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true)
                .ToListAsync(ct);

            var today = DateTime.Today;
            var todayOT = await Uow.Repository<VF03OTRequest>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true
                    && x.DeptCode == deptCode
                    && x.OTDate.Date == today
                    && x.RequestStatus != ApprovalStatus.Cancelled
                    && x.RequestStatus != ApprovalStatus.Rejected)
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .ToListAsync(ct);

            var positionCode = await Uow.Repository<F03Employee>().Query()
                .AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode)
                .Select(x => x.PositionCode)
                .FirstOrDefaultAsync(ct) ?? string.Empty;

            var approvalContext = ApprovalBuildContext.ForOT(
                employeeCode: employeeCode,
                deptCode: deptCode,
                positionCode: positionCode,
                totalOTHours: 0,
                otTypeCode: "WEEKDAY");

            var snapshotSteps = await _approvalProvider.BuildHierarchyAsync(approvalContext, ct);

            return new OTCombinedDataDto
            {
                OTForm = new OTRequestUpsertDto { DeptCode = deptCode, ScopeType = "SELECTED" },
                DeptEmployees = deptEmployees,
                ApprovalSteps = snapshotSteps,
                Balance = balance,
                LimitRules = limits.Select(r => new OTLimitRuleDto
                {
                    LimitType = r.LimitType,
                    LimitValue = r.LimitValue,
                    PositionCode = r.PositionCode,
                    DeptCode = r.DeptCode,
                    Description = r.Description,
                    LimitHours = r.LimitHours
                }).ToList(),
                RecentOTRequests = recent,
                TodayOTRequests = todayOT.Select(MapToSummary).ToList()
            };
        }

        public async Task<OTValidationResultDto> ValidateHoursAsync(
            OTRequestUpsertDto model, CancellationToken ct = default)
        {
            var result = new OTValidationResultDto();

            foreach (var emp in model.Employees)
            {
                var balance = await GetBalanceAsync(emp.EmployeeCode, model.OTDate.Year, model.OTDate.Month, ct);
                CheckLimit(result, emp, OTLimitType.Daily, balance.UsedHoursToday, balance.DailyLimit, emp.OTHours);
                CheckLimit(result, emp, OTLimitType.Monthly, balance.UsedHoursThisMonth, balance.MonthlyLimit, emp.OTHours);
                CheckLimit(result, emp, OTLimitType.Yearly, balance.UsedHoursThisYear, balance.YearlyLimit, emp.OTHours);

                if (balance.IsNearMonthlyLimit)
                    result.Warnings.Add($"{emp.EmployeeCode}: gần đạt giới hạn giờ OT tháng này ({balance.RemainingMonthly}h còn lại).");
            }

            result.IsValid = result.Errors.Count == 0;
            result.Message = result.IsValid ? "Hợp lệ." : "Có nhân viên vượt giới hạn giờ OT.";
            return result;
        }

        private static void CheckLimit(
            OTValidationResultDto result, OTEmployeeDto emp,
            OTLimitType type, decimal currentUsed, decimal limit, decimal requested)
        {
            var check = new OTEmployeeValidationDto
            {
                EmployeeCode = emp.EmployeeCode,
                EmployeeName = emp.EmployeeName,
                ViolationType = type,
                CurrentUsed = currentUsed,
                Limit = limit,
                Requested = requested
            };

            if (check.IsExceeded)
            {
                result.Errors.Add(check.Message);
                result.EmployeeResults.Add(check);
            }
        }

        public async Task<OTBalanceDto> GetBalanceAsync(
            string employeeCode, int year, int month, CancellationToken ct = default)
        {
            var today = DateTime.Today;
            var emp = await Uow.Repository<F03Employee>().Query()
                .AsNoTracking()
                .Where(e => e.EmployeeCode == employeeCode)
                .Select(e => new { e.EmployeeName, e.DeptCode, e.PositionCode })
                .FirstOrDefaultAsync(ct);

            var usedToday = await Uow.Repository<VF03OTRequest>().Query()
                .AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode && x.IsActive == true
                    && x.RequestStatus == ApprovalStatus.Approved && x.OTDate.Date == today)
                .SumAsync(x => (decimal?)x.TotalOTHours, ct) ?? 0;

            var usedThisMonth = await Uow.Repository<VF03OTRequest>().Query()
                .AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode && x.IsActive == true
                    && x.RequestStatus == ApprovalStatus.Approved
                    && x.OTDate.Year == year && x.OTDate.Month == month)
                .SumAsync(x => (decimal?)x.TotalOTHours, ct) ?? 0;

            var usedThisYear = await Uow.Repository<VF03OTRequest>().Query()
                .AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode && x.IsActive == true
                    && x.RequestStatus == ApprovalStatus.Approved && x.OTDate.Year == year)
                .SumAsync(x => (decimal?)x.TotalOTHours, ct) ?? 0;

            var dto = new OTBalanceDto
            {
                EmployeeCode = employeeCode,
                EmployeeName = emp?.EmployeeName ?? string.Empty,
                Year = year,
                Month = month,
                UsedHoursToday = usedToday,
                UsedHoursThisMonth = usedThisMonth,
                UsedHoursThisYear = usedThisYear
            };

            if (emp == null)
                return dto;

            var rules = await Uow.Repository<F03OTLimitRule>().Query()
                .AsNoTracking()
                .Where(r => r.IsActive == true
                    && (r.PositionCode == emp.PositionCode || r.DeptCode == emp.DeptCode
                        || (r.PositionCode == null && r.DeptCode == null)))
                .ToListAsync(ct);

            var dailyRule = rules.FirstOrDefault(r => r.LimitType == OTLimitType.Daily);
            var monthlyRule = rules.FirstOrDefault(r => r.LimitType == OTLimitType.Monthly);
            var yearlyRule = rules.FirstOrDefault(r => r.LimitType == OTLimitType.Yearly);

            if (dailyRule != null) dto.DailyLimit = dailyRule.LimitHours;
            if (monthlyRule != null) dto.MonthlyLimit = monthlyRule.LimitHours;
            if (yearlyRule != null) dto.YearlyLimit = yearlyRule.LimitHours;

            return dto;
        }

        public async Task<List<OTEmployeeDto>> GetDeptEmployeesAsync(
            string deptCode, CancellationToken ct = default)
        {
            var employees = await Uow.Repository<F03Employee>().Query()
                .AsNoTracking()
                .Where(e => e.DeptCode == deptCode && e.IsActive == true)
                .ToListAsync(ct);

            return employees.Select(e => new OTEmployeeDto
            {
                EmployeeCode = e.EmployeeCode,
                EmployeeName = e.EmployeeName,
                DeptCode = e.DeptCode,
                CvCode = e.PositionCode
            }).ToList();
        }

        public async Task<string> GetEmployeeDeptCodeAsync(
            string employeeCode, CancellationToken ct = default)
        {
            return await Uow.Repository<F03Employee>().Query()
                .AsNoTracking()
                .Where(e => e.EmployeeCode == employeeCode)
                .Select(e => e.DeptCode)
                .FirstOrDefaultAsync(ct) ?? string.Empty;
        }

        public async Task<List<OTBalanceDto>> GetDeptNearLimitAsync(
            string deptCode, int year, int month, CancellationToken ct = default)
        {
            var today = DateTime.Today;
            var employees = await Uow.Repository<F03Employee>().Query()
                .AsNoTracking()
                .Where(e => e.DeptCode == deptCode && e.IsActive == true)
                .Select(e => new { e.EmployeeCode, e.EmployeeName, e.PositionCode })
                .ToListAsync(ct);

            if (employees.Count == 0)
                return new List<OTBalanceDto>();

            var empCodes = employees.Select(e => e.EmployeeCode).ToList();
            var otRecords = await Uow.Repository<VF03OTRequest>().Query()
                .AsNoTracking()
                .Where(x => empCodes.Contains(x.EmployeeCode)
                    && x.IsActive == true
                    && x.RequestStatus == ApprovalStatus.Approved
                    && x.OTDate.Year == year)
                .Select(x => new { x.EmployeeCode, x.OTDate, x.TotalOTHours })
                .ToListAsync(ct);

            var positionCodes = employees.Select(e => e.PositionCode).Distinct().ToList();
            var rules = await Uow.Repository<F03OTLimitRule>().Query()
                .AsNoTracking()
                .Where(r => r.IsActive == true
                    && (positionCodes.Contains(r.PositionCode) || r.DeptCode == deptCode
                        || (r.PositionCode == null && r.DeptCode == null)))
                .ToListAsync(ct);

            var result = new List<OTBalanceDto>();
            foreach (var emp in employees)
            {
                var empRecords = otRecords.Where(r => r.EmployeeCode == emp.EmployeeCode).ToList();
                var dto = new OTBalanceDto
                {
                    EmployeeCode = emp.EmployeeCode,
                    EmployeeName = emp.EmployeeName,
                    Year = year,
                    Month = month,
                    UsedHoursToday = empRecords.Where(r => r.OTDate.Date == today).Sum(r => r.TotalOTHours ?? 0),
                    UsedHoursThisMonth = empRecords.Where(r => r.OTDate.Month == month).Sum(r => r.TotalOTHours ?? 0),
                    UsedHoursThisYear = empRecords.Sum(r => r.TotalOTHours ?? 0)
                };

                var dailyRule = ResolveRule(rules, emp.PositionCode, deptCode, OTLimitType.Daily);
                var monthlyRule = ResolveRule(rules, emp.PositionCode, deptCode, OTLimitType.Monthly);
                var yearlyRule = ResolveRule(rules, emp.PositionCode, deptCode, OTLimitType.Yearly);

                if (dailyRule != null) dto.DailyLimit = dailyRule.LimitHours;
                if (monthlyRule != null) dto.MonthlyLimit = monthlyRule.LimitHours;
                if (yearlyRule != null) dto.YearlyLimit = yearlyRule.LimitHours;

                if (dto.IsNearMonthlyLimit)
                    result.Add(dto);
            }

            return result;
        }

        private static F03OTLimitRule? ResolveRule(
            List<F03OTLimitRule> rules, string? positionCode, string deptCode, OTLimitType type)
        {
            return rules.FirstOrDefault(r => r.LimitType == type && r.PositionCode == positionCode)
                ?? rules.FirstOrDefault(r => r.LimitType == type && r.DeptCode == deptCode)
                ?? rules.FirstOrDefault(r => r.LimitType == type && r.PositionCode == null && r.DeptCode == null);
        }

        private static OTSummaryDto MapToSummary(VF03OTRequest x) => new()
        {
            Id = x.Id ?? 0,
            RequestCode = x.OTCode ?? "",
            RequestType = RequestModule.Overtime,
            TypeName = "Tăng ca",
            Status = x.RequestStatus,
            CreatedAt = x.CreatedAt ?? DateTime.Now,
            EmployeeName = x.EmployeeName,
            OTDate = x.OTDate,
            TotalHours = x.TotalOTHours ?? 0,
            EmployeeCount = 1
        };
    }
}
