using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Interfaces.OT;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Contract.ViewModels.OT;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Services.OT
{
    public class OTQueryService : BaseService<OTQueryService>, IOTQueryService
    {
        private readonly FVNWEBAPPContext _db;

        public OTQueryService(
            FVNWEBAPPContext db,
            ILogger<OTQueryService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _db = db;
        }

        // ================= COMBINED DATA (cho trang tạo đơn) =================
        public async Task<CreateOTRequestModel> GetCombinedDataAsync(
            string empCode,
            string deptCode,
            string cvCode,
            CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[OT_QUERY] GetCombinedData: {Emp}", empCode);

                var limitRulesTask = _db.F03OTLimitRules
                    .AsNoTracking()
                    .Where(x => x.IsActive == true)
                    .ToListAsync(ct);

                var approversTask = GetApproversAsync(deptCode, cvCode, ct);

                var balanceTask = GetOTBalanceAsync(empCode, DateTime.Now.Year, ct);

                await Task.WhenAll(limitRulesTask, approversTask, balanceTask);

                var approvers = await approversTask;

                var model = new CreateOTRequestModel
                {
                    EmployeeCode = empCode,
                    DeptCode = deptCode,
                    OTDate = DateTime.Today,
                    ApprovalSteps = approvers,
                    LimitRules = await limitRulesTask,
                    Balance = await balanceTask
                };

                // Set approver emails from steps
                var step3 = approvers.FirstOrDefault(x => x.Level == 3);
                var step5 = approvers.FirstOrDefault(x => x.Level == 5);
                var step6 = approvers.FirstOrDefault(x => x.Level == 6);
                var step7 = approvers.FirstOrDefault(x => x.Level == 7);

                if (step3 != null) model.Level3ApproveEmail = step3.ApproverEmail;
                if (step5 != null) model.Level5ApproveEmail = step5.ApproverEmail;
                if (step6 != null) model.Level6ApproveEmail = step6.ApproverEmail;
                if (step7 != null) model.Level7ApproveEmail = step7.ApproverEmail;

                Logger.LogDebugIf(Debug, "[OT_QUERY] CombinedData OK: {Emp}", empCode);
                return model;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT_QUERY] GetCombinedData ERROR: {Emp}", empCode);
                throw;
            }
        }

        // ================= OT BALANCE =================
        public async Task<OTBalanceDto> GetOTBalanceAsync(
            string employeeCode,
            int year,
            CancellationToken ct = default)
        {
            try
            {
                var summary = await _db.VF03OTSummary
                    .AsNoTracking()
                    .Where(x => x.EmployeeCode == employeeCode && x.WorkYear == year)
                    .FirstOrDefaultAsync(ct);

                var limits = await _db.F03OTLimitRules
                    .AsNoTracking()
                    .Where(x => x.IsActive == true)
                    .ToListAsync(ct);

                var dailyLimit = limits.FirstOrDefault(x => x.LimitType == OTLimitType.Daily)?.LimitValue ?? 4m;
                var weeklyLimit = limits.FirstOrDefault(x => x.LimitType == OTLimitType.Weekly)?.LimitValue ?? 40m;
                var yearlyLimit = limits.FirstOrDefault(x => x.LimitType == OTLimitType.Yearly)?.LimitValue ?? 200m;
                var specialLimit = limits.FirstOrDefault(x => x.LimitType == OTLimitType.Special)?.LimitValue ?? 900m;

                return new OTBalanceDto
                {
                    EmployeeCode = employeeCode,
                    Year = year,
                    UsedHours = summary?.TotalApprovedHours ?? 0,
                    PendingHours = summary?.TotalPendingHours ?? 0,
                    YearlyLimit = yearlyLimit,
                    SpecialLimit = specialLimit,
                    RemainingYearly = yearlyLimit - (summary?.TotalApprovedHours ?? 0),
                    DailyLimit = dailyLimit,
                    WeeklyLimit = weeklyLimit
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT_QUERY] GetOTBalance ERROR: {Emp}", employeeCode);
                return new OTBalanceDto { EmployeeCode = employeeCode, Year = year };
            }
        }

        // ================= RECENT REQUESTS =================
        public async Task<List<OTRequestViewModel>> GetRecentOTRequestsAsync(
            string employeeCode,
            int limit = 10,
            CancellationToken ct = default)
        {
            try
            {
                var data = await _db.VF03OTRequest
                    .AsNoTracking()
                    .Where(x => x.EmployeeCode == employeeCode && x.IsActive == true)
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(limit)
                    .ToListAsync(ct);

                return data.Select(MapToViewModel).ToList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT_QUERY] GetRecentOTRequests ERROR: {Emp}", employeeCode);
                return new List<OTRequestViewModel>();
            }
        }

        // ================= GET DETAIL =================
        public async Task<OTRequestViewModel?> GetOTDetailAsync(
            int otRequestId,
            CancellationToken ct = default)
        {
            try
            {
                var data = await _db.VF03OTRequest
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == otRequestId, ct);

                if (data == null) return null;

                var employees = await _db.F03OTEmployee
                    .AsNoTracking()
                    .Where(x => x.OTRequestId == otRequestId)
                    .ToListAsync(ct);

                var vm = MapToViewModel(data);
                vm.Employees = employees.Select(e => new OTEmployeeModel
                {
                    EmployeeCode = e.EmployeeCode,
                    EmployeeName = e.EmployeeName,
                    DeptCode = e.DeptCode,
                    DeptName = e.DeptName,
                    OTHours = e.OTHours,
                    OTTypeCode = e.OTTypeCode
                }).ToList();

                return vm;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT_QUERY] GetOTDetail ERROR: {Id}", otRequestId);
                return null;
            }
        }

        // ================= PENDING APPROVALS =================
        public async Task<List<OTRequestViewModel>> GetPendingApprovalsAsync(
            string approverEmail,
            int level,
            CancellationToken ct = default)
        {
            try
            {
                var query = _db.VF03OTRequest
                    .AsNoTracking()
                    .Where(x => x.IsActive == true &&
                                x.RequestStatus != OTStatus.Approved &&
                                x.RequestStatus != OTStatus.Rejected &&
                                x.RequestStatus != OTStatus.Cancelled);

                // Filter by level
                query = level switch
                {
                    3 => query.Where(x => x.Level3ApproveEmail == approverEmail && x.Level3IsApprove == null),
                    5 => query.Where(x => x.Level5ApproveEmail == approverEmail && x.Level3IsApprove == true && x.Level5IsApprove == null),
                    6 => query.Where(x => x.Level6ApproveEmail == approverEmail && x.Level5IsApprove == true && x.Level6IsApprove == null),
                    7 => query.Where(x => x.Level7ApproveEmail == approverEmail && x.Level6IsApprove == true && x.Level7IsApprove == null),
                    _ => query.Where(x =>
                        (x.Level3ApproveEmail == approverEmail && x.Level3IsApprove == null) ||
                        (x.Level5ApproveEmail == approverEmail && x.Level3IsApprove == true && x.Level5IsApprove == null) ||
                        (x.Level6ApproveEmail == approverEmail && x.Level5IsApprove == true && x.Level6IsApprove == null) ||
                        (x.Level7ApproveEmail == approverEmail && x.Level6IsApprove == true && x.Level7IsApprove == null))
                };

                var data = await query.OrderByDescending(x => x.OTDate).ToListAsync(ct);
                return data.Select(MapToViewModel).ToList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT_QUERY] GetPendingApprovals ERROR: {Email}", approverEmail);
                return new List<OTRequestViewModel>();
            }
        }

        // ================= COUNT PENDING =================
        public async Task<int> CountPendingApprovalsAsync(
            string approverEmail,
            CancellationToken ct = default)
        {
            try
            {
                return await _db.VF03OTRequest
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.IsActive == true &&
                        x.RequestStatus != OTStatus.Approved &&
                        x.RequestStatus != OTStatus.Rejected &&
                        x.RequestStatus != OTStatus.Cancelled &&
                        (
                            (x.Level3ApproveEmail == approverEmail && x.Level3IsApprove == null) ||
                            (x.Level5ApproveEmail == approverEmail && x.Level3IsApprove == true && x.Level5IsApprove == null) ||
                            (x.Level6ApproveEmail == approverEmail && x.Level5IsApprove == true && x.Level6IsApprove == null) ||
                            (x.Level7ApproveEmail == approverEmail && x.Level6IsApprove == true && x.Level7IsApprove == null)
                        ), ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT_QUERY] CountPending ERROR: {Email}", approverEmail);
                return 0;
            }
        }

        // ================= PAGED LIST =================
        public async Task<PaginationResult<OTRequestViewModel>> GetPagedOTRequestsAsync(
            string? deptCode,
            string? status,
            DateTime? fromDate,
            DateTime? toDate,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            try
            {
                var query = _db.VF03OTRequest.AsNoTracking().Where(x => x.IsActive == true);

                if (!string.IsNullOrEmpty(deptCode))
                    query = query.Where(x => x.DeptCode == deptCode);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(x => x.RequestStatus == status);

                if (fromDate.HasValue)
                    query = query.Where(x => x.OTDate >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(x => x.OTDate <= toDate.Value);

                var total = await query.CountAsync(ct);

                var data = await query
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct);

                return new PaginationResult<OTRequestViewModel>(
                    data.Select(MapToViewModel).ToList(),
                    total, page, pageSize);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT_QUERY] GetPaged ERROR");
                return PaginationResult<OTRequestViewModel>.Fail("Lỗi tải danh sách OT.");
            }
        }

        // ================= DASHBOARD =================
        public async Task<OTDashboardViewModel> GetDashboardAsync(
            string employeeCode,
            string deptCode,
            int? permissionCode,
            CancellationToken ct = default)
        {
            try
            {
                var email = await ResolveApproverEmailAsync(employeeCode, ct);

                var pendingCount = string.IsNullOrEmpty(email) ? 0
                    : await CountPendingApprovalsAsync(email, ct);

                var balance = await GetOTBalanceAsync(employeeCode, DateTime.Now.Year, ct);
                var recent = await GetRecentOTRequestsAsync(employeeCode, 5, ct);

                var widgets = new List<WidgetCounterDto>
                {
                    new() {
                        Title = "Đơn OT chờ duyệt",
                        Value = pendingCount.ToString(),
                        Icon = "Icons.Material.Filled.HourglassEmpty",
                        Color = "warning",
                        Link = "/ot/approvals"
                    },
                    new() {
                        Title = "Giờ OT đã dùng",
                        Value = balance.UsedHours.ToString("N1"),
                        Icon = "Icons.Material.Filled.AccessTime",
                        Color = "info",
                        Link = "/ot/history"
                    },
                    new() {
                        Title = "Giờ OT còn lại",
                        Value = balance.RemainingYearly.ToString("N1"),
                        Icon = "Icons.Material.Filled.TimerOutlined",
                        Color = balance.RemainingYearly < 20 ? "error" : "success",
                        Link = "/ot/balance"
                    }
                };

                return new OTDashboardViewModel
                {
                    Widgets = widgets,
                    Balance = balance,
                    RecentRequests = recent,
                    PendingApprovalCount = pendingCount
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT_QUERY] GetDashboard ERROR: {Emp}", employeeCode);
                return new OTDashboardViewModel();
            }
        }

        // ================= VALIDATE OT HOURS =================
        public async Task<OTValidationResultDto> ValidateOTHoursAsync(
            string employeeCode,
            DateTime otDate,
            decimal hours,
            string otType,
            CancellationToken ct = default)
        {
            try
            {
                var limits = await _db.F03OTLimitRules
                    .AsNoTracking()
                    .Where(x => x.IsActive == true)
                    .ToListAsync(ct);

                var dailyLimit = limits.FirstOrDefault(x => x.LimitType == OTLimitType.Daily)?.LimitValue ?? 4m;
                var weeklyLimit = limits.FirstOrDefault(x => x.LimitType == OTLimitType.Weekly)?.LimitValue ?? 40m;
                var yearlyLimit = limits.FirstOrDefault(x => x.LimitType == OTLimitType.Yearly)?.LimitValue ?? 200m;

                // Tính giờ đã OT trong ngày
                var todayUsed = await _db.F03OTEmployee
                    .AsNoTracking()
                    .Where(x => x.EmployeeCode == employeeCode)
                    .Join(_db.F03OTRequest.Where(r =>
                            r.OTDate.Date == otDate.Date &&
                            r.RequestStatus != OTStatus.Rejected &&
                            r.RequestStatus != OTStatus.Cancelled),
                        e => e.OTRequestId, r => r.Id, (e, r) => e.OTHours)
                    .SumAsync(ct);

                // Tính giờ trong tuần
                var weekStart = otDate.AddDays(-(int)otDate.DayOfWeek);
                var weekEnd = weekStart.AddDays(7);
                var weekUsed = await _db.F03OTEmployee
                    .AsNoTracking()
                    .Where(x => x.EmployeeCode == employeeCode)
                    .Join(_db.F03OTRequest.Where(r =>
                            r.OTDate >= weekStart && r.OTDate < weekEnd &&
                            r.RequestStatus != OTStatus.Rejected &&
                            r.RequestStatus != OTStatus.Cancelled),
                        e => e.OTRequestId, r => r.Id, (e, r) => e.OTHours)
                    .SumAsync(ct);

                var balance = await GetOTBalanceAsync(employeeCode, otDate.Year, ct);

                var violations = new List<OTEmployeeValidationDto>();

                if (todayUsed + hours > dailyLimit)
                    violations.Add(new OTEmployeeValidationDto
                    {
                        EmployeeCode = employeeCode,
                        ViolationType = OTLimitType.Daily,
                        CurrentUsed = todayUsed,
                        Limit = dailyLimit,
                        Requested = hours
                    });

                if (weekUsed + hours > weeklyLimit)
                    violations.Add(new OTEmployeeValidationDto
                    {
                        EmployeeCode = employeeCode,
                        ViolationType = OTLimitType.Weekly,
                        CurrentUsed = weekUsed,
                        Limit = weeklyLimit,
                        Requested = hours
                    });

                if (balance.UsedHours + hours > yearlyLimit)
                    violations.Add(new OTEmployeeValidationDto
                    {
                        EmployeeCode = employeeCode,
                        ViolationType = OTLimitType.Yearly,
                        CurrentUsed = balance.UsedHours,
                        Limit = yearlyLimit,
                        Requested = hours
                    });

                return new OTValidationResultDto
                {
                    IsValid = violations.Count == 0,
                    Violations = violations,
                    Message = violations.Count > 0
                        ? "Vượt giới hạn giờ OT theo quy định."
                        : "Hợp lệ"
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT_QUERY] ValidateOTHours ERROR");
                return new OTValidationResultDto { IsValid = false, Message = "Lỗi kiểm tra giờ OT." };
            }
        }

        // ================= GET APPROVERS =================
        public async Task<List<OTApprovalStep>> GetApproversAsync(
            string deptCode,
            string cvCode,
            CancellationToken ct = default)
        {
            try
            {
                var approvers = await _db.F03OTApprover
                    .AsNoTracking()
                    .Where(x => x.IsActive == true &&
                                (x.DeptCode == deptCode || x.DeptCode == "ALL" || string.IsNullOrEmpty(x.DeptCode)))
                    .OrderBy(x => x.ApproveLevel)
                    .ToListAsync(ct);

                return approvers.Select(a => new OTApprovalStep
                {
                    Level = a.ApproveLevel,
                    LevelName = GetLevelName(a.ApproveLevel),
                    ApproverName = a.ApproverName,
                    ApproverEmail = a.ApproverEmail,
                    ApproverCode = a.ApproverCode
                }).ToList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT_QUERY] GetApprovers ERROR: {Dept}", deptCode);
                return new List<OTApprovalStep>();
            }
        }

        // ================= HELPERS =================
        private async Task<string> ResolveApproverEmailAsync(string employeeCode, CancellationToken ct)
        {
            return await _db.F03OTApprover
                .AsNoTracking()
                .Where(x => x.ApproverCode == employeeCode && x.IsActive == true)
                .Select(x => x.ApproverEmail)
                .FirstOrDefaultAsync(ct) ?? string.Empty;
        }

        private static OTRequestViewModel MapToViewModel(VF03OTRequest x) => new()
        {
            Id = x.Id ?? 0,
            EmployeeCode = x.EmployeeCode ?? string.Empty,
            EmployeeName = x.EmployeeName ?? string.Empty,
            DeptCode = x.DeptCode ?? string.Empty,
            DeptName = x.DeptName ?? string.Empty,
            OTDate = x.OTDate,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            TotalOTHours = x.TotalOTHours ?? 0,
            OTTypeCode = x.OTTypeCode ?? string.Empty,
            OTTypeName = x.OTTypeName ?? string.Empty,
            Reason = x.Reason ?? string.Empty,
            RequestStatus = x.RequestStatus ?? OTStatus.Pending,
            Level3IsApprove = x.Level3IsApprove,
            Level3ApproveName = x.Level3ApproveName,
            Level3ApproveTime = x.Level3ApproveTime,
            Level5IsApprove = x.Level5IsApprove,
            Level5ApproveName = x.Level5ApproveName,
            Level5ApproveTime = x.Level5ApproveTime,
            Level6IsApprove = x.Level6IsApprove,
            Level6ApproveName = x.Level6ApproveName,
            Level6ApproveTime = x.Level6ApproveTime,
            Level7IsApprove = x.Level7IsApprove,
            Level7ApproveName = x.Level7ApproveName,
            Level7ApproveTime = x.Level7ApproveTime,
            CreatedAt = x.CreatedAt ?? DateTime.Now
        };

        private static string GetLevelName(int level) => level switch
        {
            3 => "Sub-leader / Leader",
            5 => "Ast. Chief / Chief",
            6 => "A.MG / MG",
            7 => "GM",
            _ => $"Level {level}"
        };
    }
}