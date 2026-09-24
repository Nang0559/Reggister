using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Application.Interfaces.Histories;
using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Application.Maps;
using FVN_REGISTER.Infrastructure.Services.Common;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.LimitRuleDtos;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Requests.OT;

using FVN_REGISTER.Core.Repositories;

using Microsoft.EntityFrameworkCore;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;

namespace FVN_REGISTER.Infrastructure.Services.OT
{
    public class OTQueryService
        : BaseRequestQueryService<OTSummaryDto, OTBalanceDto, OTEmployeeDto, OTRequestDto>,
          IOTQueryService
    {
        private readonly IApprovalProvider<OTRequestSubject> _approvalProvider;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuthorizationService _authorization;

        protected override RequestModule Module => RequestModule.Overtime;

        public OTQueryService(
            IUnitOfWork uow,
            IApprovalHistoryService historyService,
            IAttachmentService attachmentService,
            ICurrentUserService currentUserService,
            IApprovalProvider<OTRequestSubject> approvalProvider,
            IAuthorizationService authorization)
            : base(uow, historyService, attachmentService, currentUserService)
        {
            _approvalProvider = approvalProvider;
            _currentUser = currentUserService;
            _authorization = authorization;
        }

        protected override async Task<OTRequestDto?> GetHeaderByIdAsync(int requestId, CancellationToken ct)
        {
            var entity = await Uow.Repository<F03OTRequest>().Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == requestId && x.IsActive == true, ct);

            if (entity == null)
                return null;

            var user = _currentUser.GetCurrentUser();
            if (user == null || !await _authorization.CanAccessAsync(user, SecurityFunctionCodes.OTView, entity.EmployeeCode, entity.DeptCode, ct))
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

            var user = _currentUser.GetCurrentUser();
            if (user == null)
                return new PaginationResult<OTSummaryDto>(new List<OTSummaryDto>(), 0, page, pageSize);

            var scope = await _authorization.GetScopeAsync(user.UserId, SecurityFunctionCodes.OTView, ct);
            if (scope != AuthorizationScopeCodes.All)
            {
                var managedEmployees = await _authorization.GetManagedEmployeesAsync(user.UserId, ct);
                var managedEmployeeCodes = managedEmployees
                    .Select(x => x.EmployeeCode)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                var managedDeptCodes = managedEmployees
                    .Select(x => x.DeptCode)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (scope == AuthorizationScopeCodes.Own || scope == AuthorizationScopeCodes.Employee)
                    query = query.Where(x => x.EmployeeCode == user.EmployeeCode || managedEmployeeCodes.Contains(x.EmployeeCode));
                else if (scope == AuthorizationScopeCodes.Department)
                    query = query.Where(x => x.DeptCode == user.DeptCode || managedDeptCodes.Contains(x.DeptCode));
                else
                    query = query.Where(x => false);
            }

            if (!string.IsNullOrWhiteSpace(deptCode) && scope == AuthorizationScopeCodes.All)
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
            var user = _currentUser.GetCurrentUser();
            if (user == null)
                return new List<OTRequestDto>();

            var scope = await _authorization.GetScopeAsync(user.UserId, SecurityFunctionCodes.OTView, ct);
            var managedEmployees = await _authorization.GetManagedEmployeesAsync(user.UserId, ct);
            var managedEmployeeCodes = managedEmployees
                .Select(x => x.EmployeeCode)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var canSeeRequestedDepartment = scope == AuthorizationScopeCodes.All
                || string.Equals(user.DeptCode, deptCode, StringComparison.OrdinalIgnoreCase)
                || managedEmployees.Any(x => string.Equals(x.DeptCode, deptCode, StringComparison.OrdinalIgnoreCase));

            if (!canSeeRequestedDepartment)
                return new List<OTRequestDto>();

            var data = await Uow.Repository<VF03OTRequest>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true
                    && x.DeptCode == deptCode
                    && (scope == AuthorizationScopeCodes.All
                        || scope == AuthorizationScopeCodes.Department
                        || managedEmployeeCodes.Contains(x.EmployeeCode)
                        || x.EmployeeCode == user.EmployeeCode)
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
                requestId: 0,
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
            var preview = await GetLimitPreviewAsync(model, ct);
            var result = new OTValidationResultDto
            {
                IsValid = preview.IsValid,
                Message = preview.IsValid ? "Hợp lệ." : "Có giới hạn OT bị vượt."
            };

            result.Errors.AddRange(preview.Errors);
            result.Warnings.AddRange(preview.Warnings);

            foreach (var employee in preview.Employees.Where(x => x.IsExceeded))
            {
                if (employee.ExceedsWeekly)
                    result.EmployeeResults.Add(new OTEmployeeValidationDto
                    {
                        EmployeeCode = employee.EmployeeCode,
                        EmployeeName = employee.EmployeeName,
                        ViolationType = OTLimitType.Weekly,
                        CurrentUsed = employee.UsedHoursThisWeek,
                        Limit = employee.WeeklyLimit ?? 0m,
                        Requested = employee.RequestedHours
                    });

                if (employee.ExceedsMonthly)
                    result.EmployeeResults.Add(new OTEmployeeValidationDto
                    {
                        EmployeeCode = employee.EmployeeCode,
                        EmployeeName = employee.EmployeeName,
                        ViolationType = OTLimitType.Monthly,
                        CurrentUsed = employee.UsedHoursThisMonth,
                        Limit = employee.MonthlyLimit ?? 0m,
                        Requested = employee.RequestedHours
                    });

                if (employee.ExceedsYearly)
                    result.EmployeeResults.Add(new OTEmployeeValidationDto
                    {
                        EmployeeCode = employee.EmployeeCode,
                        EmployeeName = employee.EmployeeName,
                        ViolationType = OTLimitType.Yearly,
                        CurrentUsed = employee.UsedHoursThisYear,
                        Limit = employee.YearlyLimit ?? 0m,
                        Requested = employee.RequestedHours
                    });
            }

            return result;
        }

        private static void CheckLimit(
            OTValidationResultDto result, OTEmployeeDto emp,
            OTLimitType type, decimal currentUsed, decimal limit, decimal requested)
        {
            if (limit <= 0)
                return;

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
                .Where(e => e.EmployeeCode == employeeCode && e.IsActive == true)
                .Select(e => new { e.EmployeeName, e.DeptCode, e.PositionCode })
                .FirstOrDefaultAsync(ct);

            var dto = new OTBalanceDto
            {
                EmployeeCode = employeeCode,
                EmployeeName = emp?.EmployeeName ?? string.Empty,
                Year = year,
                Month = month
            };

            if (emp == null)
                return dto;

            var weekOffset = (7 + (int)today.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            var weekStart = today.Date.AddDays(-weekOffset);
            var weekEnd = weekStart.AddDays(7);

            var usedData = await Uow.Repository<F03OTEmployee>().Query()
                .AsNoTracking()
                .Where(e => e.EmployeeCode == employeeCode
                    && e.IsActive == true
                    && e.OTRequest.IsActive == true
                    && e.OTRequest.RequestStatus != ApprovalStatus.Rejected
                    && e.OTRequest.RequestStatus != ApprovalStatus.Cancelled)
                .Select(e => new
                {
                    EffectiveHours = e.ActualHours.HasValue && e.ActualHours.Value > 0
                        ? e.ActualHours.Value
                        : e.OTHours,
                    e.OTRequest.OTDate
                })
                .ToListAsync(ct);

            dto.UsedHoursToday = usedData
                .Where(x => x.OTDate.Date == today)
                .Sum(x => x.EffectiveHours);
            dto.UsedHoursThisWeek = usedData
                .Where(x => x.OTDate >= weekStart && x.OTDate < weekEnd)
                .Sum(x => x.EffectiveHours);
            dto.UsedHoursThisMonth = usedData
                .Where(x => x.OTDate.Year == year && x.OTDate.Month == month)
                .Sum(x => x.EffectiveHours);
            dto.UsedHoursThisYear = usedData
                .Where(x => x.OTDate.Year == year)
                .Sum(x => x.EffectiveHours);

            var rules = await Uow.Repository<F03OTLimitRule>().Query()
                .AsNoTracking()
                .Where(r => r.IsActive == true)
                .ToListAsync(ct);

            var dailyRule = ResolveEmployeeRule(rules, employeeCode, emp.PositionCode, emp.DeptCode, OTLimitType.Daily);
            var weeklyRule = ResolveEmployeeRule(rules, employeeCode, emp.PositionCode, emp.DeptCode, OTLimitType.Weekly);
            var monthlyRule = ResolveEmployeeRule(rules, employeeCode, emp.PositionCode, emp.DeptCode, OTLimitType.Monthly);
            var yearlyRule = ResolveEmployeeRule(rules, employeeCode, emp.PositionCode, emp.DeptCode, OTLimitType.Yearly);

            dto.DailyLimit = dailyRule?.LimitHours ?? 0m;
            dto.WeeklyLimit = weeklyRule?.LimitHours;
            dto.MonthlyLimit = monthlyRule?.LimitHours ?? 0m;
            dto.YearlyLimit = yearlyRule?.LimitHours ?? 0m;

            return dto;
        }

        public async Task<OTLimitPreviewDto> GetLimitPreviewAsync(
            OTRequestUpsertDto model, CancellationToken ct = default)
        {
            var preview = new OTLimitPreviewDto
            {
                OTDate = model.OTDate.Date,
                RequestedHours = model.Employees?.Sum(x => x.OTHours) ?? 0m
            };

            if (model.Employees == null || model.Employees.Count == 0)
                return preview;

            var codes = model.Employees
                .Select(x => x.EmployeeCode)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            var employees = await Uow.Repository<F03Employee>().Query()
                .AsNoTracking()
                .Where(e => e.IsActive == true && codes.Contains(e.EmployeeCode ?? string.Empty))
                .Select(e => new { e.EmployeeCode, e.EmployeeName, e.DeptCode, e.PositionCode })
                .ToListAsync(ct);

            if (employees.Count != codes.Count)
            {
                var missing = codes.Except(employees.Select(x => x.EmployeeCode)).ToList();
                preview.Errors.Add($"Không tìm thấy nhân viên: {string.Join(", ", missing)}.");
            }

            var deptCodes = employees.Select(x => x.DeptCode).Distinct().ToList();
            if (deptCodes.Count > 1)
            {
                preview.Errors.Add("Đăng ký OT theo phòng ban chỉ cho phép các nhân viên cùng một phòng ban.");
                return preview;
            }

            var deptCode = deptCodes.FirstOrDefault() ?? model.DeptCode ?? string.Empty;
            var blockCode = await Uow.Repository<F03Department>().Query()
                .AsNoTracking()
                .Where(d => d.DeptCode == deptCode && d.IsActive == true)
                .Select(d => d.BlockCode)
                .FirstOrDefaultAsync(ct);

            var weekOffset = (7 + (int)model.OTDate.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            var weekStart = model.OTDate.Date.AddDays(-weekOffset);
            var weekEnd = weekStart.AddDays(7);
            var monthStart = new DateTime(model.OTDate.Year, model.OTDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1);
            var yearStart = new DateTime(model.OTDate.Year, 1, 1);
            var yearEnd = yearStart.AddYears(1);

            var blockDeptCodes = string.IsNullOrWhiteSpace(blockCode)
                ? new List<string>()
                : await Uow.Repository<F03Department>().Query()
                    .AsNoTracking()
                    .Where(d => d.IsActive == true && d.BlockCode == blockCode)
                    .Select(d => d.DeptCode)
                    .ToListAsync(ct);

            var used = await Uow.Repository<F03OTEmployee>().Query()
                .AsNoTracking()
                .Where(e => e.IsActive == true
                    && e.OTRequest.IsActive == true
                    && e.OTRequest.RequestStatus != ApprovalStatus.Rejected
                    && e.OTRequest.RequestStatus != ApprovalStatus.Cancelled
                    && (e.OTRequest.OTDate >= yearStart && e.OTRequest.OTDate < yearEnd)
                    && (codes.Contains(e.EmployeeCode)
                        || (deptCode != string.Empty && e.OTRequest.DeptCode == deptCode)
                        || blockDeptCodes.Contains(e.OTRequest.DeptCode ?? string.Empty)))
                .Select(e => new
                {
                    e.EmployeeCode,
                    e.OTRequest.DeptCode,
                    EffectiveHours = e.ActualHours.HasValue && e.ActualHours.Value > 0
                        ? e.ActualHours.Value
                        : e.OTHours,
                    e.OTRequest.OTDate
                })
                .ToListAsync(ct);

            var rules = await Uow.Repository<F03OTLimitRule>().Query()
                .AsNoTracking()
                .Where(r => r.IsActive == true)
                .ToListAsync(ct);

            foreach (var emp in employees)
            {
                var requested = model.Employees
                    .Where(x => x.EmployeeCode == emp.EmployeeCode)
                    .Sum(x => x.OTHours);

                var empUsed = used.Where(x => x.EmployeeCode == emp.EmployeeCode).ToList();
                var row = new OTLimitEmployeePreviewDto
                {
                    EmployeeCode = emp.EmployeeCode,
                    EmployeeName = emp.EmployeeName,
                    DeptCode = emp.DeptCode,
                    RequestedHours = requested,
                    UsedHoursThisWeek = empUsed.Where(x => x.OTDate >= weekStart && x.OTDate < weekEnd).Sum(x => x.EffectiveHours),
                    UsedHoursThisMonth = empUsed.Where(x => x.OTDate >= monthStart && x.OTDate < monthEnd).Sum(x => x.EffectiveHours),
                    UsedHoursThisYear = empUsed.Where(x => x.OTDate >= yearStart && x.OTDate < yearEnd).Sum(x => x.EffectiveHours)
                };

                row.WeeklyLimit = ResolveEmployeeRule(rules, emp.EmployeeCode, emp.PositionCode, emp.DeptCode, OTLimitType.Weekly)?.LimitHours;
                row.MonthlyLimit = ResolveEmployeeRule(rules, emp.EmployeeCode, emp.PositionCode, emp.DeptCode, OTLimitType.Monthly)?.LimitHours;
                row.YearlyLimit = ResolveEmployeeRule(rules, emp.EmployeeCode, emp.PositionCode, emp.DeptCode, OTLimitType.Yearly)?.LimitHours;

                preview.Employees.Add(row);
                AddPreviewMessages(preview, row);
            }

            if (!string.IsNullOrWhiteSpace(deptCode))
            {
                var deptUsed = used.Where(x => x.DeptCode == deptCode).ToList();
                var deptName = await Uow.Repository<F03Department>().Query()
                    .AsNoTracking()
                    .Where(d => d.DeptCode == deptCode)
                    .Select(d => d.DeptName)
                    .FirstOrDefaultAsync(ct) ?? deptCode;

                var deptRequest = model.Employees
                    .Where(x => employees.Any(e => e.EmployeeCode == x.EmployeeCode && e.DeptCode == deptCode))
                    .Sum(x => x.OTHours);

                var deptPreview = new OTLimitScopePreviewDto
                {
                    ScopeType = OTLimitScopeType.Department,
                    ScopeCode = deptCode,
                    ScopeName = deptName,
                    RequestedHours = deptRequest,
                    UsedHoursThisWeek = deptUsed.Where(x => x.OTDate >= weekStart && x.OTDate < weekEnd).Sum(x => x.EffectiveHours),
                    UsedHoursThisMonth = deptUsed.Where(x => x.OTDate >= monthStart && x.OTDate < monthEnd).Sum(x => x.EffectiveHours),
                    UsedHoursThisYear = deptUsed.Where(x => x.OTDate >= yearStart && x.OTDate < yearEnd).Sum(x => x.EffectiveHours)
                };
                deptPreview.WeeklyLimit = ResolveAggregateRule(rules, OTLimitScopeType.Department, deptCode, OTLimitType.Weekly)?.LimitHours;
                deptPreview.MonthlyLimit = ResolveAggregateRule(rules, OTLimitScopeType.Department, deptCode, OTLimitType.Monthly)?.LimitHours;
                deptPreview.YearlyLimit = ResolveAggregateRule(rules, OTLimitScopeType.Department, deptCode, OTLimitType.Yearly)?.LimitHours;
                preview.Department = deptPreview;
                AddPreviewMessages(preview, deptPreview);
            }

            if (!string.IsNullOrWhiteSpace(blockCode))
            {
                var blockUsed = used.Where(x => blockDeptCodes.Contains(x.DeptCode ?? string.Empty)).ToList();
                var blockRequest = model.Employees
                    .Where(x => employees.Any(e => e.EmployeeCode == x.EmployeeCode && blockDeptCodes.Contains(e.DeptCode ?? string.Empty)))
                    .Sum(x => x.OTHours);

                var blockPreview = new OTLimitScopePreviewDto
                {
                    ScopeType = OTLimitScopeType.Block,
                    ScopeCode = blockCode,
                    ScopeName = blockCode,
                    RequestedHours = blockRequest,
                    UsedHoursThisWeek = blockUsed.Where(x => x.OTDate >= weekStart && x.OTDate < weekEnd).Sum(x => x.EffectiveHours),
                    UsedHoursThisMonth = blockUsed.Where(x => x.OTDate >= monthStart && x.OTDate < monthEnd).Sum(x => x.EffectiveHours),
                    UsedHoursThisYear = blockUsed.Where(x => x.OTDate >= yearStart && x.OTDate < yearEnd).Sum(x => x.EffectiveHours)
                };
                blockPreview.WeeklyLimit = ResolveAggregateRule(rules, OTLimitScopeType.Block, blockCode, OTLimitType.Weekly)?.LimitHours;
                blockPreview.MonthlyLimit = ResolveAggregateRule(rules, OTLimitScopeType.Block, blockCode, OTLimitType.Monthly)?.LimitHours;
                blockPreview.YearlyLimit = ResolveAggregateRule(rules, OTLimitScopeType.Block, blockCode, OTLimitType.Yearly)?.LimitHours;
                preview.Block = blockPreview;
                AddPreviewMessages(preview, blockPreview);
            }

            return preview;
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

        private async Task<string?> GetEmployeePositionCodeAsync(
            string employeeCode, CancellationToken ct = default)
        {
            return await Uow.Repository<F03Employee>().Query()
                .AsNoTracking()
                .Where(e => e.EmployeeCode == employeeCode)
                .Select(e => e.PositionCode)
                .FirstOrDefaultAsync(ct);
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
                .Where(x => empCodes.Contains(x.EmployeeCode?? string.Empty)
                    && x.IsActive == true
                    && x.RequestStatus == ApprovalStatus.Approved
                    && x.OTDate.Year == year)
                .Select(x => new { x.EmployeeCode, x.OTDate, x.TotalOTHours })
                .ToListAsync(ct);

            var positionCodes = employees.Select(e => e.PositionCode).Distinct().ToList();
            var rules = await Uow.Repository<F03OTLimitRule>().Query()
                .AsNoTracking()
                .Where(r => r.IsActive == true
                    && (positionCodes.Contains(r.PositionCode??string.Empty) || r.DeptCode == deptCode
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

                var dailyRule = ResolveEmployeeRule(rules, emp.EmployeeCode, emp.PositionCode, deptCode, OTLimitType.Daily);
                var monthlyRule = ResolveEmployeeRule(rules, emp.EmployeeCode, emp.PositionCode, deptCode, OTLimitType.Monthly);
                var yearlyRule = ResolveEmployeeRule(rules, emp.EmployeeCode, emp.PositionCode, deptCode, OTLimitType.Yearly);

                if (dailyRule != null) dto.DailyLimit = dailyRule.LimitHours;
                if (monthlyRule != null) dto.MonthlyLimit = monthlyRule.LimitHours;
                if (yearlyRule != null) dto.YearlyLimit = yearlyRule.LimitHours;

                if (dto.IsNearMonthlyLimit)
                    result.Add(dto);
            }

            return result;
        }

        private static F03OTLimitRule? ResolveEmployeeRule(
            List<F03OTLimitRule> rules,
            string employeeCode,
            string? positionCode,
            string deptCode,
            OTLimitType type)
        {
            return rules
                .Where(r => r.ScopeType == OTLimitScopeType.Employee
                    && r.LimitType == type
                    && (string.IsNullOrWhiteSpace(r.EmployeeCode) || r.EmployeeCode == employeeCode)
                    && (r.DeptCode == null || r.DeptCode == deptCode)
                    && (r.PositionCode == null || r.PositionCode == positionCode))
                .OrderByDescending(r => !string.IsNullOrWhiteSpace(r.EmployeeCode))
                .ThenByDescending(r => r.PositionCode != null && r.DeptCode != null)
                .ThenByDescending(r => r.DeptCode != null)
                .ThenByDescending(r => r.PositionCode != null)
                .FirstOrDefault();
        }

        private static F03OTLimitRule? ResolveAggregateRule(
            List<F03OTLimitRule> rules,
            OTLimitScopeType scopeType,
            string scopeCode,
            OTLimitType type)
            => rules.FirstOrDefault(r =>
                r.ScopeType == scopeType &&
                r.LimitType == type &&
                r.ScopeCode == scopeCode);

        private static void AddPreviewMessages(OTLimitPreviewDto preview, OTLimitEmployeePreviewDto row)
        {
            if (row.ExceedsWeekly)
                preview.Errors.Add($"{row.EmployeeCode}: vượt giới hạn OT tuần.");
            if (row.ExceedsMonthly)
                preview.Errors.Add($"{row.EmployeeCode}: vượt giới hạn OT tháng.");
            if (row.ExceedsYearly)
                preview.Errors.Add($"{row.EmployeeCode}: vượt giới hạn OT năm.");
        }

        private static void AddPreviewMessages(OTLimitPreviewDto preview, OTLimitScopePreviewDto row)
        {
            if (row.ExceedsWeekly)
                preview.Errors.Add($"{row.ScopeType}: {row.ScopeCode} vượt giới hạn OT tuần.");
            if (row.ExceedsMonthly)
                preview.Errors.Add($"{row.ScopeType}: {row.ScopeCode} vượt giới hạn OT tháng.");
            if (row.ExceedsYearly)
                preview.Errors.Add($"{row.ScopeType}: {row.ScopeCode} vượt giới hạn OT năm.");
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
