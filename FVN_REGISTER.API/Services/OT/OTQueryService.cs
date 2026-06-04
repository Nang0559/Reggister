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

        // ════════════════════════════════════════════════════════════════════
        // COMBINED DATA — dùng để render trang tạo đơn OT
        // ════════════════════════════════════════════════════════════════════
        public async Task<CombinedOTViewModel> GetCombinedDataAsync(
    string employeeCode,
    string deptCode,
    string cvCode,
    int year,
    CancellationToken ct = default)
        {
            Logger.LogDebugIf(Debug, "[OT_QUERY] GetCombinedData: {Emp}", employeeCode);

            var now = DateTime.Now;

            // 1. Chạy tuần tự từng Task một để tránh xung đột dùng chung một instance _db (DbContext)
            var balance = await GetBalanceInternalAsync(employeeCode, year, now.Month, ct);

            var rules = await _db.F03OTLimitRules
                                  .AsNoTracking()
                                  .Where(x => x.IsActive)
                                  .ToListAsync(ct);

            var steps = await BuildApprovalStepsAsync(deptCode, cvCode, ct);

            var deptEmps = await GetDeptEmployeesAsync(deptCode, ct);

            // 2. Khởi tạo form dữ liệu gửi đi (Mapping dữ liệu bổ trợ vào như bạn đã định nghĩa)
            var form = new CreateOTRequestModel
            {
                EmployeeCode = employeeCode,
                DeptCode = deptCode,
                CvCode = cvCode,
                OTDate = DateTime.Today,
                OTTypeCode = OTTypeConst.Weekday, // Đảm bảo hằng số này khớp với Frontend của bạn
                LimitRules = rules,
                ApprovalSteps = steps,
                Balance = balance
            };

            // 3. Trả về Wrapper Model chứa tất cả dữ liệu sạch sẽ
            return new CombinedOTViewModel
            {
                OTForm = form,
                Balance = balance,
                LimitRules = rules,
                ApprovalSteps = steps,
                DeptEmployees = deptEmps
            };
        }

        // ════════════════════════════════════════════════════════════════════
        // PENDING SUMMARY — chỉ trả về số đếm (badge/widget)
        // ════════════════════════════════════════════════════════════════════
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

            // Mỗi level chỉ chờ duyệt khi level trước đã xong
            var lv3 = baseQ.Where(x =>
                x.Level3ApproveEmail == approverEmail
                && x.Level3IsApprove == null);

            var lv5 = baseQ.Where(x =>
                x.Level5ApproveEmail == approverEmail
                && (x.Level3IsApprove == true || string.IsNullOrEmpty(x.Level3ApproveEmail))
                && x.Level5IsApprove == null);

            var lv6 = baseQ.Where(x =>
                x.Level6ApproveEmail == approverEmail
                && x.Level5IsApprove == true
                && x.Level6IsApprove == null);

            var lv7 = baseQ.Where(x =>
                x.Level7ApproveEmail == approverEmail
                && x.Level6IsApprove == true
                && x.Level7IsApprove == null);

            var counts = await Task.WhenAll(
                lv3.CountAsync(ct),
                lv5.CountAsync(ct),
                lv6.CountAsync(ct),
                lv7.CountAsync(ct));

            var result = new List<OTPendingGroup>
            {
                new() { ApproverLevel = 3, LevelName = "Sub-leader / Leader", Count = counts[0] },
                new() { ApproverLevel = 5, LevelName = "Ast. Chief / Chief",  Count = counts[1] },
                new() { ApproverLevel = 6, LevelName = "A.MG / MG",           Count = counts[2] },
                new() { ApproverLevel = 7, LevelName = "GM",                  Count = counts[3] }
            };

            return result.Where(x => x.Count > 0).ToList();
        }

        // ════════════════════════════════════════════════════════════════════
        // PENDING DETAILS — kèm danh sách đơn cụ thể
        // ════════════════════════════════════════════════════════════════════
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
                    LevelName     = "Sub-leader / Leader",
                    Requests      = data
                        .Where(x => x.Level3ApproveEmail == approverEmail
                                 && x.Level3IsApprove == null)
                        .Select(MapToViewModel).ToList()
                },
                new()
                {
                    ApproverLevel = 5,
                    LevelName     = "Ast. Chief / Chief",
                    Requests      = data
                        .Where(x => x.Level5ApproveEmail == approverEmail
                                 && (x.Level3IsApprove == true
                                     || string.IsNullOrEmpty(x.Level3ApproveEmail))
                                 && x.Level5IsApprove == null)
                        .Select(MapToViewModel).ToList()
                },
                new()
                {
                    ApproverLevel = 6,
                    LevelName     = "A.MG / MG",
                    Requests      = data
                        .Where(x => x.Level6ApproveEmail == approverEmail
                                 && x.Level5IsApprove == true
                                 && x.Level6IsApprove == null)
                        .Select(MapToViewModel).ToList()
                },
                new()
                {
                    ApproverLevel = 7,
                    LevelName     = "GM",
                    Requests      = data
                        .Where(x => x.Level7ApproveEmail == approverEmail
                                 && x.Level6IsApprove == true
                                 && x.Level7IsApprove == null)
                        .Select(MapToViewModel).ToList()
                }
            };

            foreach (var g in result) g.Count = g.Requests.Count;
            return result.Where(x => x.Count > 0).ToList();
        }

        // ════════════════════════════════════════════════════════════════════
        // RECENT HISTORY
        // ════════════════════════════════════════════════════════════════════
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

        // ════════════════════════════════════════════════════════════════════
        // VALIDATE HOURS — trả về chi tiết vi phạm theo từng nhân viên
        // ════════════════════════════════════════════════════════════════════
        public async Task<OTValidationResultDto> ValidateHoursAsync(
            CreateOTRequestModel model,
            CancellationToken ct = default)
        {
            var result = new OTValidationResultDto { IsValid = true };

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

            // Lấy dữ liệu giờ đã dùng của tất cả nhân viên trong 1 query
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
                var empData = usedData
                    .Where(u => u.EmployeeCode == emp.EmployeeCode)
                    .ToList();

                decimal usedToday = empData
                    .Where(u => u.OTDate.Date == otDate.Date)
                    .Sum(u => u.OTHours);

                decimal usedMonth = empData
                    .Where(u => u.OTDate.Year == year && u.OTDate.Month == month)
                    .Sum(u => u.OTHours);

                decimal usedYear = empData.Sum(u => u.OTHours);
                decimal requested = emp.OTHours;

                var empResult = new OTEmployeeValidationDto
                {
                    EmployeeCode = emp.EmployeeCode,
                    EmployeeName = emp.EmployeeName,
                    Requested = requested
                };

                if (usedToday + requested > dailyLimit)
                {
                    empResult.ViolationType = OTLimitType.Daily;
                    empResult.CurrentUsed = usedToday;
                    empResult.Limit = dailyLimit;
                    result.IsValid = false;
                    result.Errors.Add(empResult.Message);
                }
                else if (usedYear + requested > yearlyLimit)
                {
                    empResult.ViolationType = OTLimitType.Yearly;
                    empResult.CurrentUsed = usedYear;
                    empResult.Limit = yearlyLimit;
                    result.IsValid = false;
                    result.Errors.Add(empResult.Message);
                }
                else if (usedMonth + requested > monthlyLimit)
                {
                    // Warning — không block nhưng cảnh báo
                    empResult.ViolationType = OTLimitType.Weekly;
                    empResult.CurrentUsed = usedMonth;
                    empResult.Limit = monthlyLimit;
                    result.Warnings.Add(empResult.Message);
                }

                result.EmployeeResults.Add(empResult);
            }

            if (!result.IsValid)
                result.Message = string.Join("; ", result.Errors);
            else if (result.Warnings.Any())
                result.Message = string.Join("; ", result.Warnings);

            return result;
        }

        // ════════════════════════════════════════════════════════════════════
        // APPROVERS — ưu tiên phòng ban, fallback về "ALL"
        // ════════════════════════════════════════════════════════════════════
        public async Task<List<F03OTApprover>> GetApproversAsync(
            int level,
            string deptCode,
            CancellationToken ct = default)
        {
            var list = await _db.F03OTApprovers
                .AsNoTracking()
                .Where(x => x.ApproveLevel == level && x.IsActive)
                .ToListAsync(ct);

            var deptSpecific = list.Where(x => x.DeptCode == deptCode).ToList();
            return deptSpecific.Any()
                ? deptSpecific
                : list.Where(x => x.DeptCode == "ALL").ToList();
        }

        // ════════════════════════════════════════════════════════════════════
        // DASHBOARD WIDGETS
        // ════════════════════════════════════════════════════════════════════
        public async Task<List<WidgetCounterDto>> GetDashboardWidgetsAsync(
            string approverEmail,
            string deptCode,
            CancellationToken ct = default)
        {
            var today = DateTime.Today;

            var pendingCount = await _db.VF03OTRequests.CountAsync(x =>
                x.IsActive == true
                && OTStatus.ActiveStatuses.Contains(x.RequestStatus!)
                && (x.Level3ApproveEmail == approverEmail
                 || x.Level5ApproveEmail == approverEmail
                 || x.Level6ApproveEmail == approverEmail
                 || x.Level7ApproveEmail == approverEmail), ct);

            var otToday = await _db.VF03OTRequests.CountAsync(x =>
                x.IsActive == true
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

        // ════════════════════════════════════════════════════════════════════
        // GET BALANCE — public method khớp với interface
        // ════════════════════════════════════════════════════════════════════
        public Task<OTBalanceDto> GetBalanceAsync(
            string employeeCode,
            int year,
            int month,
            CancellationToken ct = default)
            => GetBalanceInternalAsync(employeeCode, year, month, ct);

        // ════════════════════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ════════════════════════════════════════════════════════════════════

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
            // Công nhân (cvCode 0003): cần đủ bước 3, 5, 6, 7
            // Văn phòng: chỉ cần bước 5, 6, 7
            var levels = cvCode == "0003"
                ? new[] { 3, 5, 6, 7 }
                : new[] { 5, 6, 7 };

            var steps = new List<OTApprovalStep>();
            foreach (var level in levels)
            {
                var approvers = await GetApproversAsync(level, deptCode, ct);
                var first = approvers.FirstOrDefault();
                steps.Add(new OTApprovalStep
                {
                    Level = level,
                    LevelName = GetLevelName(level),
                    RoleName = first?.RoleName ?? "",
                    ApproverCode = first?.ApproverCode,
                    ApproverName = first?.ApproverName,
                    ApproverEmail = first?.ApproverEmail,
                    IsRequired = true
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

        // ── PUBLIC vì OTService cần gọi để map GetDetailsAsync ────────────
        public OTRequestViewModel MapToViewModel(VF03OTRequest x) => new()
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
        public async Task<PaginationResult<OTRequestViewModel>> GetPagedAsync(
    string? deptCode,
    string? status,
    DateTime? fromDate,
    DateTime? toDate,
    int page,
    int pageSize,
    CancellationToken ct = default)
        {
            var query = _db.VF03OTRequests
                .AsNoTracking()
                .Where(x => x.IsActive == true);

            if (!string.IsNullOrWhiteSpace(deptCode))
                query = query.Where(x => x.DeptCode == deptCode);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(x => x.RequestStatus == status);

            if (fromDate.HasValue)
                query = query.Where(x => x.OTDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.OTDate <= toDate.Value);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.OTDate)
                .ThenByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PaginationResult<OTRequestViewModel>(
                items.Select(MapToViewModel).ToList(),
                totalCount,
                page,
                pageSize);
        }
        private static List<OTApprovalStep> BuildStepsFromView(VF03OTRequest x) => new()
        {
            new()
            {
                Level         = 3,
                LevelName     = "Sub-leader / Leader",
                ApproverEmail = x.Level3ApproveEmail,
                ApproverName  = x.Level3ApproveName,
                ApproverCode  = x.Level3ApproveCode,
                IsApproved    = x.Level3IsApprove,
                ApproveTime   = x.Level3ApproveTime,
                Comment       = x.Level3Comment,
                IsRequired    = !string.IsNullOrEmpty(x.Level3ApproveEmail),
                IsSkipped     = string.IsNullOrEmpty(x.Level3ApproveEmail)
            },
            new()
            {
                Level         = 5,
                LevelName     = "Ast. Chief / Chief",
                ApproverEmail = x.Level5ApproveEmail,
                ApproverName  = x.Level5ApproveName,
                ApproverCode  = x.Level5ApproveCode,
                IsApproved    = x.Level5IsApprove,
                ApproveTime   = x.Level5ApproveTime,
                Comment       = x.Level5Comment,
                IsRequired    = true
            },
            new()
            {
                Level         = 6,
                LevelName     = "A.MG / MG",
                ApproverEmail = x.Level6ApproveEmail,
                ApproverName  = x.Level6ApproveName,
                ApproverCode  = x.Level6ApproveCode,
                IsApproved    = x.Level6IsApprove,
                ApproveTime   = x.Level6ApproveTime,
                Comment       = x.Level6Comment,
                IsRequired    = true
            },
            new()
            {
                Level         = 7,
                LevelName     = "GM",
                ApproverEmail = x.Level7ApproveEmail,
                ApproverName  = x.Level7ApproveName,
                ApproverCode  = x.Level7ApproveCode,
                IsApproved    = x.Level7IsApprove,
                ApproveTime   = x.Level7ApproveTime,
                Comment       = x.Level7Comment,
                IsRequired    = !string.IsNullOrEmpty(x.Level7ApproveEmail),
                IsSkipped     = string.IsNullOrEmpty(x.Level7ApproveEmail)
            }
        };
    }
}