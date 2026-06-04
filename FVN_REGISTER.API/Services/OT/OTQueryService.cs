// ============================================================
// FVN_REGISTER.API/Services/OT/OTQueryService.cs
// ============================================================
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Interfaces.OT;
using FVN_REGISTER.Contract.Interfaces.Repositores;
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

        // ── COMBINED DATA ─────────────────────────────────────────────────────
        public async Task<CombinedOTViewModel> GetCombinedDataAsync(
            string employeeCode,
            string deptCode,
            string cvCode,
            int year,
            CancellationToken ct = default)
        {
            Logger.LogDebugIf(Debug, "[OT_QUERY] GetCombinedData: {Emp}", employeeCode);

            var now = DateTime.Now;

            // Chạy song song các task độc lập
            var balanceTask = GetBalanceInternalAsync(employeeCode, year, now.Month, ct);
            var rulesTask = _db.F03OTLimitRules
                                    .AsNoTracking()
                                    .Where(x => x.IsActive)
                                    .ToListAsync(ct);
            var stepsTask = BuildApprovalStepsAsync(deptCode, cvCode, ct);
            var deptEmpsTask = GetDeptEmployeesAsync(deptCode, ct);

            await Task.WhenAll(balanceTask, rulesTask, stepsTask, deptEmpsTask);

            var form = new CreateOTRequestModel
            {
                EmployeeCode = employeeCode,
                DeptCode = deptCode,
                CvCode = cvCode,
                OTDate = DateTime.Today,
                OTTypeCode = OTTypeConst.Weekday,
                LimitRules = await rulesTask,
                ApprovalSteps = await stepsTask,
                Balance = await balanceTask
            };

            return new CombinedOTViewModel
            {
                OTForm = form,
                Balance = await balanceTask,
                LimitRules = await rulesTask,
                ApprovalSteps = await stepsTask,
                DeptEmployees = await deptEmpsTask
            };
        }

        // ── PENDING SUMMARY ────────────────────────────────────────────────────
        public async Task<List<OTPendingGroup>> GetPendingSummaryAsync(
            string approverEmail,
            CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(approverEmail)) return new();

            var baseQ = _db.VF03OTRequests
                .AsNoTracking()
                .Where(x => x.IsActive == true
                         && !new[] { OTStatus.Approved, OTStatus.Rejected, OTStatus.Cancelled }
                                .Contains(x.RequestStatus!));

            var lv3 = baseQ.Where(x => x.Level3ApproveEmail == approverEmail && x.Level3IsApprove == null);
            var lv5 = baseQ.Where(x => x.Level5ApproveEmail == approverEmail
                                    && x.Level3IsApprove == true
                                    && x.Level5IsApprove == null);
            var lv6 = baseQ.Where(x => x.Level6ApproveEmail == approverEmail
                                    && x.Level5IsApprove == true
                                    && x.Level6IsApprove == null);
            var lv7 = baseQ.Where(x => x.Level7ApproveEmail == approverEmail
                                    && x.Level6IsApprove == true
                                    && x.Level7IsApprove == null);

            var counts = await Task.WhenAll(
                lv3.CountAsync(ct),
                lv5.CountAsync(ct),
                lv6.CountAsync(ct),
                lv7.CountAsync(ct));

            var result = new List<OTPendingGroup>
            {
                new() { ApproverLevel = 3, Count = counts[0] },
                new() { ApproverLevel = 5, Count = counts[1] },
                new() { ApproverLevel = 6, Count = counts[2] },
                new() { ApproverLevel = 7, Count = counts[3] }
            };

            return result.Where(x => x.Count > 0).ToList();
        }

        // ── PENDING DETAILS ────────────────────────────────────────────────────
        public async Task<List<OTPendingGroup>> GetPendingDetailsAsync(
            string approverEmail,
            CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(approverEmail)) return new();

            var data = await _db.VF03OTRequests
                .AsNoTracking()
                .Where(x => x.IsActive == true
                         && !new[] { OTStatus.Approved, OTStatus.Rejected, OTStatus.Cancelled }
                                .Contains(x.RequestStatus!)
                         && (x.Level3ApproveEmail == approverEmail
                          || x.Level5ApproveEmail == approverEmail
                          || x.Level6ApproveEmail == approverEmail
                          || x.Level7ApproveEmail == approverEmail))
                .ToListAsync(ct);

            var result = new List<OTPendingGroup>
            {
                new()
                {
                    ApproverLevel = 3,
                    Requests = data
                        .Where(x => x.Level3ApproveEmail == approverEmail && x.Level3IsApprove == null)
                        .Select(MapToViewModel).ToList()
                },
                new()
                {
                    ApproverLevel = 5,
                    Requests = data
                        .Where(x => x.Level5ApproveEmail == approverEmail
                                 && x.Level3IsApprove == true
                                 && x.Level5IsApprove == null)
                        .Select(MapToViewModel).ToList()
                },
                new()
                {
                    ApproverLevel = 6,
                    Requests = data
                        .Where(x => x.Level6ApproveEmail == approverEmail
                                 && x.Level5IsApprove == true
                                 && x.Level6IsApprove == null)
                        .Select(MapToViewModel).ToList()
                },
                new()
                {
                    ApproverLevel = 7,
                    Requests = data
                        .Where(x => x.Level7ApproveEmail == approverEmail
                                 && x.Level6IsApprove == true
                                 && x.Level7IsApprove == null)
                        .Select(MapToViewModel).ToList()
                }
            };

            foreach (var g in result) g.Count = g.Requests.Count;
            return result.Where(x => x.Count > 0).ToList();
        }

        // ── RECENT HISTORY ─────────────────────────────────────────────────────
        public async Task<List<OTRequestViewModel>> GetRecentHistoryAsync(
            string employeeCode,
            int limit,
            CancellationToken ct = default)
        {
            var data = await _db.VF03OTRequests
                .AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode && x.IsActive == true)
                .OrderByDescending(x => x.CreatedAt)
                .Take(limit)
                .ToListAsync(ct);

            return data.Select(MapToViewModel).ToList();
        }

        // ── VALIDATE HOURS ─────────────────────────────────────────────────────
        public async Task<OTValidationResultDto> ValidateHoursAsync(
            CreateOTRequestModel model,
            CancellationToken ct = default)
        {
            var result = new OTValidationResultDto { IsValid = true };

            // Lấy limit rules từ DB
            var rules = await _db.F03OTLimitRules
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToDictionaryAsync(x => x.LimitType, ct);

            decimal dailyLimit = GetLimit(rules, OTLimitType.Daily, 4m);
            decimal monthlyLimit = GetLimit(rules, OTLimitType.Weekly, 40m);
            decimal yearlyLimit = GetLimit(rules, OTLimitType.Yearly, 200m);

            var otDate = model.OTDate;
            var year = otDate.Year;
            var month = otDate.Month;
            var empCodes = model.Employees.Select(e => e.EmployeeCode).Distinct().ToList();

            // Lấy giờ OT đã dùng theo ngày / tháng / năm cho từng nhân viên
            // (chỉ tính các đơn Approved hoặc đang trong luồng Active)
            var validStatuses = OTStatus.ActiveStatuses
                .Concat(new[] { OTStatus.Approved })
                .ToArray();

            var usedData = await _db.F03OTEmployees
                .AsNoTracking()
                .Where(e => empCodes.Contains(e.EmployeeCode)
                         && validStatuses.Contains(e.OTRequest.RequestStatus)
                         && e.OTRequest.OTDate.Year == year
                         && e.OTRequest.IsActive == true)
                .Select(e => new
                {
                    e.EmployeeCode,
                    e.OTHours,
                    e.OTRequest.OTDate
                })
                .ToListAsync(ct);

            foreach (var emp in model.Employees)
            {
                var empData = usedData.Where(u => u.EmployeeCode == emp.EmployeeCode).ToList();

                decimal usedToday = empData.Where(u => u.OTDate.Date == otDate.Date).Sum(u => u.OTHours);
                decimal usedMonth = empData.Where(u => u.OTDate.Year == year && u.OTDate.Month == month).Sum(u => u.OTHours);
                decimal usedYear = empData.Sum(u => u.OTHours);
                decimal requested = emp.OTHours;

                var empResult = new OTEmployeeValidationDto
                {
                    EmployeeCode = emp.EmployeeCode,
                    EmployeeName = emp.EmployeeName,
                    Requested = requested
                };

                // Check Daily
                if (usedToday + requested > dailyLimit)
                {
                    empResult.ViolationType = OTLimitType.Daily;
                    empResult.CurrentUsed = usedToday;
                    empResult.Limit = dailyLimit;
                    result.IsValid = false;
                    result.Errors.Add(empResult.Message);
                }
                // Check Monthly
                else if (usedMonth + requested > monthlyLimit)
                {
                    empResult.ViolationType = OTLimitType.Weekly;
                    empResult.CurrentUsed = usedMonth;
                    empResult.Limit = monthlyLimit;
                    result.Warnings.Add(empResult.Message);
                }
                // Check Yearly
                else if (usedYear + requested > yearlyLimit)
                {
                    empResult.ViolationType = OTLimitType.Yearly;
                    empResult.CurrentUsed = usedYear;
                    empResult.Limit = yearlyLimit;
                    result.IsValid = false;
                    result.Errors.Add(empResult.Message);
                }

                result.EmployeeResults.Add(empResult);
            }

            if (!result.IsValid)
                result.Message = string.Join("; ", result.Errors);

            return result;
        }

        // ── APPROVERS ──────────────────────────────────────────────────────────
        public async Task<List<F03OTApprover>> GetApproversAsync(
            int level,
            string deptCode,
            CancellationToken ct = default)
        {
            var list = await _db.F03OTApprovers
                .AsNoTracking()
                .Where(x => x.ApproveLevel == level && x.IsActive)
                .ToListAsync(ct);

            // Ưu tiên approver của phòng ban, nếu không có lấy "ALL"
            var deptSpecific = list.Where(x => x.DeptCode == deptCode).ToList();
            return deptSpecific.Any()
                ? deptSpecific
                : list.Where(x => x.DeptCode == "ALL").ToList();
        }

        // ── DASHBOARD WIDGETS ──────────────────────────────────────────────────
        public async Task<List<WidgetCounterDto>> GetDashboardWidgetsAsync(
            string approverEmail,
            string deptCode,
            CancellationToken ct = default)
        {
            var today = DateTime.Today;

            var pendingCount = await _db.VF03OTRequests
                .CountAsync(x => x.IsActive == true
                              && OTStatus.ActiveStatuses.Contains(x.RequestStatus!)
                              && (x.Level3ApproveEmail == approverEmail
                               || x.Level5ApproveEmail == approverEmail
                               || x.Level6ApproveEmail == approverEmail
                               || x.Level7ApproveEmail == approverEmail), ct);

            var otToday = await _db.VF03OTRequests
                .CountAsync(x => x.IsActive == true
                              && x.DeptCode == deptCode
                              && x.OTDate == today
                              && x.RequestStatus == OTStatus.Approved, ct);

            return new List<WidgetCounterDto>
            {
                new()
                {
                    Title = "Đơn OT chờ duyệt",
                    Value = pendingCount.ToString(),
                    Icon  = "Icons.Material.Filled.HourglassEmpty",
                    Color = "warning",
                    Link  = "/ot/approvals"
                },
                new()
                {
                    Title = "OT hôm nay (đã duyệt)",
                    Value = otToday.ToString(),
                    Icon  = "Icons.Material.Filled.AccessTime",
                    Color = "info",
                    Link  = "/ot/today"
                }
            };
        }

        // ── INTERNAL HELPERS ───────────────────────────────────────────────────
        private async Task<OTBalanceDto> GetBalanceInternalAsync(
            string employeeCode,
            int year,
            int month,
            CancellationToken ct)
        {
            var today = DateTime.Today;
            var validStatuses = OTStatus.ActiveStatuses
                .Concat(new[] { OTStatus.Approved })
                .ToArray();

            var empHours = await _db.F03OTEmployees
                .AsNoTracking()
                .Where(e => e.EmployeeCode == employeeCode
                         && e.OTRequest.IsActive == true
                         && validStatuses.Contains(e.OTRequest.RequestStatus)
                         && e.OTRequest.OTDate.Year == year)
                .Select(e => new { e.OTHours, e.OTRequest.OTDate })
                .ToListAsync(ct);

            var rules = await _db.F03OTLimitRules
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToDictionaryAsync(x => x.LimitType, ct);

            var emp = await _db.VF03employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode, ct);

            return new OTBalanceDto
            {
                EmployeeCode = employeeCode,
                EmployeeName = emp?.EmployeeName ?? "",
                Year = year,
                Month = month,
                UsedHoursToday = empHours.Where(e => e.OTDate.Date == today).Sum(e => e.OTHours),
                UsedHoursThisMonth = empHours.Where(e => e.OTDate.Month == month).Sum(e => e.OTHours),
                UsedHoursThisYear = empHours.Sum(e => e.OTHours),
                DailyLimit = GetLimit(rules, OTLimitType.Daily, 4m),
                MonthlyLimit = GetLimit(rules, OTLimitType.Weekly, 40m),
                YearlyLimit = GetLimit(rules, OTLimitType.Yearly, 200m)
            };
        }

        private async Task<List<OTApprovalStep>> BuildApprovalStepsAsync(
            string deptCode,
            string cvCode,
            CancellationToken ct)
        {
            // Công nhân (cvCode 0003) cần đủ 4 bước (3, 5, 6, 7)
            // Văn phòng chỉ cần 3 bước (5, 6, 7)
            var levels = cvCode == "0003"
                ? new[] { 3, 5, 6, 7 }
                : new[] { 5, 6, 7 };

            var steps = new List<OTApprovalStep>();
            foreach (var level in levels)
            {
                var approvers = await GetApproversAsync(level, deptCode, ct);
                steps.Add(new OTApprovalStep
                {
                    Level = level,
                    LevelName = GetLevelName(level),
                    ApproverCode = approvers.FirstOrDefault()?.ApproverCode,
                    ApproverName = approvers.FirstOrDefault()?.ApproverName,
                    ApproverEmail = approvers.FirstOrDefault()?.ApproverEmail
                });
            }
            return steps;
        }

        private async Task<List<OTEmployeeModel>> GetDeptEmployeesAsync(
            string deptCode,
            CancellationToken ct)
        {
            return await _db.VF03employees
                .AsNoTracking()
                .Where(e => e.DeptCode == deptCode && e.IsActive)
                .Select(e => new OTEmployeeModel
                {
                    EmployeeCode = e.EmployeeCode,
                    EmployeeName = e.EmployeeName,
                    DeptCode = e.DeptCode,
                    DeptName = e.DeptName,
                    CvCode = e.Cvcode,
                    OTHours = 0,
                    OTRateMultiplier = 1.5m
                })
                .ToListAsync(ct);
        }

        private static decimal GetLimit(
            Dictionary<string, F03OTLimitRule> rules,
            string limitType,
            decimal defaultValue)
            => rules.TryGetValue(limitType, out var rule) ? rule.LimitValue : defaultValue;

        private static string GetLevelName(int level) => level switch
        {
            3 => "Sub-leader / Leader",
            5 => "Ast. Chief / Chief",
            6 => "A.MG / MG",
            7 => "GM",
            _ => $"Level {level}"
        };

        internal OTRequestViewModel MapToViewModel(VF03OTRequest x) => new()
        {
            Id = x.Id ?? 0,
            OTCode = x.OTCode ?? "",
            EmployeeCode = x.EmployeeCode ?? "",
            EmployeeName = x.EmployeeName,
            DeptCode = x.DeptCode ?? "",
            DeptName = x.DeptName,
            OTDate = x.OTDate,
            OTTypeCode = x.OTTypeCode ?? "",
            OTTypeName = x.OTTypeName ?? OTTypeConst.GetDisplayName(x.OTTypeCode ?? ""),
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            TotalOTHours = x.TotalOTHours ?? 0,
            Reason = x.Reason,
            ScopeType = x.ScopeType ?? "SELECTED",
            RequestStatus = x.RequestStatus ?? "",
            StatusText = OTStatus.GetDisplayName(x.RequestStatus ?? ""),
            StatusColor = OTStatus.GetColor(x.RequestStatus ?? ""),
            EmployeeCount = x.EmployeeCount ?? 0,
            CreatedAt = x.CreatedAt ?? DateTime.Now,
            IsActive = x.IsActive ?? true,
            ApprovalSteps = BuildStepsFromView(x)
        };

        private static List<OTApprovalStep> BuildStepsFromView(VF03OTRequest x) =>
            new()
            {
                new() { Level = 3, LevelName = "Sub-leader / Leader",
                    ApproverEmail = x.Level3ApproveEmail, ApproverName = x.Level3ApproveName,
                    ApproverCode  = x.Level3ApproveCode,  IsApproved    = x.Level3IsApprove,
                    ApprovedAt    = x.Level3ApproveTime,  Comment       = x.Level3Comment },
                new() { Level = 5, LevelName = "Ast. Chief / Chief",
                    ApproverEmail = x.Level5ApproveEmail, ApproverName = x.Level5ApproveName,
                    ApproverCode  = x.Level5ApproveCode,  IsApproved    = x.Level5IsApprove,
                    ApprovedAt    = x.Level5ApproveTime,  Comment       = x.Level5Comment },
                new() { Level = 6, LevelName = "A.MG / MG",
                    ApproverEmail = x.Level6ApproveEmail, ApproverName = x.Level6ApproveName,
                    ApproverCode  = x.Level6ApproveCode,  IsApproved    = x.Level6IsApprove,
                    ApprovedAt    = x.Level6ApproveTime,  Comment       = x.Level6Comment },
                new() { Level = 7, LevelName = "GM",
                    ApproverEmail = x.Level7ApproveEmail, ApproverName = x.Level7ApproveName,
                    ApproverCode  = x.Level7ApproveCode,  IsApproved    = x.Level7IsApprove,
                    ApprovedAt    = x.Level7ApproveTime,  Comment       = x.Level7Comment },
            };
    }
}
}