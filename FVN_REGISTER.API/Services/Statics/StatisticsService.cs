namespace FVN_REGISTER.API.Services.Statics
{
    using FVN_REGISTER.Contract.Dtos;
    using FVN_REGISTER.Contract.Interfaces.Statics;
    using FVN_REGISTER.Contract.Models;
    using FVN_REGISTER.Contract.Utils;
    using FVN_REGISTER.Contract.ViewModels;
    using FVN_REGISTER.Core.Configurations;
    using FVN_REGISTER.Core.Logging;
    using FVN_REGISTER.Core.Services;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
 

    public class StatisticsService
     : BaseService<StatisticsService>, IStatisticsService
    {
        private readonly FVNWEBAPPContext _db;

        public StatisticsService(
            FVNWEBAPPContext db,
            ILogger<StatisticsService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _db = db;
        }

        // ================= TOTAL EMPLOYEE =================
        public async Task<int> GetTotalEmployeesAsync(CancellationToken ct = default)
        {
            try
            {
                var count = await _db.F03employees
                    .CountAsync(x => x.IsActive == true, ct);

                Logger.LogDebugIf(Debug, "[STAT] TotalEmployees: {Count}", count);
                return count;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[STAT] GetTotalEmployees ERROR");
                throw;
            }
        }

        // ================= PRESENT TODAY =================
        public async Task<int> GetPresentTodayAsync(CancellationToken ct = default)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                var count = await _db.VwCurrentlyPresentEmployees
                    .Where(x => x.Date == today)
                    .Select(x => x.EmployeeId)
                    .Distinct()
                    .CountAsync(ct);

                Logger.LogDebugIf(Debug, "[STAT] PresentToday: {Count}", count);
                return count;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[STAT] GetPresentToday ERROR");
                throw;
            }
        }

        // ================= HOLIDAY COUNT =================
        public async Task<int> GetHolidayCountAsync(int year, CancellationToken ct = default)
        {
            try
            {
                var count = await _db.CompanyHolidays
                    .CountAsync(x => x.Year == year, ct);

                Logger.LogDebugIf(Debug, "[STAT] HolidayCount {Year}: {Count}", year, count);
                return count;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[STAT] GetHolidayCount ERROR");
                throw;
            }
        }

        // ================= WORK YEAR =================
        public async Task<WorkYearStatsViewModel> GetWorkYearStatsAsync(CancellationToken ct = default)
        {
            try
            {
                var years = await _db.F03workYears
                    .AsNoTracking()
                    .ToListAsync(ct);

                var active = years.FirstOrDefault(x => x.IsActive == true);

                var result = new WorkYearStatsViewModel
                {
                    TotalWorkYears = years.Count,
                    ActiveWorkYears = years.Count(x => x.IsActive == true),
                    CurrentYear = DateTime.Now.Year,
                    ActiveYear = active?.WorkYear,
                    AllYears = years
                        .Where(x => x.WorkYear.HasValue)
                        .Select(x => x.WorkYear!.Value)
                        .ToList()
                };

                Logger.LogDebugIf(Debug,
                    "[STAT] WorkYearStats: Total={Total}, Active={Active}",
                    result.TotalWorkYears,
                    result.ActiveWorkYears);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[STAT] GetWorkYearStats ERROR");
                throw;
            }
        }

        // ================= DEPARTMENT =================
        public async Task<LeaveStatisticsViewModel> GetDepartmentStatisticsAsync(
            string deptCode,
            CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[STAT] GetDepartment: {Dept}", deptCode);

                var stats = await GetLeaveStatisticsAsync(false, ct);

                var result = stats
                    .FirstOrDefault(x => x.DepartmentId == deptCode)
                    ?? new LeaveStatisticsViewModel();

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[STAT] GetDepartmentStatistics ERROR: {Dept}", deptCode);
                throw;
            }
        }

        // ================= TIME RANGE =================
        public async Task<List<LeaveStatisticsViewModel>> GetStatisticsByTimeRangeAsync(
            string[]? departments,
            TimeRange timeRange,
            CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[STAT] TimeRange: {Range}", timeRange);

                var today = DateTime.Today;

                var fromDate = timeRange switch
                {
                    TimeRange.Week => today.AddDays(-7),
                    TimeRange.Month => today.AddMonths(-1),
                    TimeRange.Year => today.AddYears(-1),
                    _ => today
                };

                var query = _db.VF03leaveDays
                    .AsNoTracking()
                    .Where(x => x.RegisterDate >= fromDate && x.RegisterDate <= today);

                if (departments != null && departments.Length > 0)
                {
                    query = query.Where(x => departments.Contains(x.DeptCode));
                }

                var leaves = await query.ToListAsync(ct);

                var result = leaves
                    .GroupBy(x => new { x.DeptCode, x.DeptName })
                    .Select(g => new LeaveStatisticsViewModel
                    {
                        DepartmentId = g.Key.DeptCode,
                        DepartmentName = g.Key.DeptName,
                        ApprovedLeaveCount = g.Count(x => LeaveStatusHelper.IsApproved(x.RequestStatus)),
                        PendingLeaveCount = g.Count(x => LeaveStatusHelper.IsInProcess(x.RequestStatus)),
                        TotalLeaveCount = g.Count()
                    })
                    .ToList();

                Logger.LogInfoIf(Debug,
                    "[STAT] TimeRange DONE: {Count} departments",
                    result.Count);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[STAT] GetStatisticsByTimeRange ERROR");
                throw;
            }
        }

        // ================= MAIN (giữ nguyên của bạn) =================
        public async Task<List<LeaveStatisticsViewModel>> GetLeaveStatisticsAsync(
      bool includeCompanyTotal = false,
      CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[STAT] GetLeaveStatistics START");

                var today = DateOnly.FromDateTime(DateTime.Today);

                // 1. LẤY DỮ LIỆU THÔ (Raw) - Đảm bảo EF Core không phải ép kiểu sai tại đây
                // Lưu ý: Select ra các thuộc tính ẩn danh để EF tự mapping đúng kiểu của DB (int)
                var employeesRaw = await _db.VF03leaveDays
                    .AsNoTracking()
                    .Where(x => x.IsActive == true)
                    .Select(x => new { x.EmployeeCode, x.DeptCode, x.DeptName })
                    .ToListAsync(ct);

                var leavesRaw = await _db.VF03leaveDays
                    .AsNoTracking()
                    .Where(x => x.IsActive == true)
                    .Select(x => new { x.EmployeeCode, x.RequestStatus })
                    .ToListAsync(ct);

                var presentTodayRaw = await _db.VwCurrentlyPresentEmployees
                    .AsNoTracking()
                    .Where(x => x.Date == today)
                    .Select(x => new { x.EmployeeId, x.DeptCode }) // DeptCode ở đây cũng là int
                    .Distinct()
                    .ToListAsync(ct);

                // 2. XỬ LÝ LOGIC TRÊN MEMORY (Lúc này ép kiểu .ToString() thoải mái)
                var grouped = employeesRaw
                    .GroupBy(e => new { e.DeptCode, e.DeptName })
                    .Select(g =>
                    {
                        var deptEmpCodes = g.Select(x => x.EmployeeCode).ToHashSet();
                        var deptLeaves = leavesRaw.Where(l => deptEmpCodes.Contains(l.EmployeeCode)).ToList();

                        var approved = deptLeaves.Count(l => LeaveStatusHelper.IsApproved(l.RequestStatus));
                        var pending = deptLeaves.Count(l => LeaveStatusHelper.IsInProcess(l.RequestStatus));

                        // So sánh kiểu int trước khi lấy số lượng
                        var present = presentTodayRaw.Count(p => p.DeptCode == g.Key.DeptCode);
                        var totalEmp = deptEmpCodes.Count;

                        return new LeaveStatisticsViewModel
                        {
                            // Chuyển đổi int -> string an toàn cho ViewModel
                            DepartmentId = g.Key.DeptCode.ToString(),
                            DepartmentName = g.Key.DeptName,
                            TotalEmployees = totalEmp,
                            TotalPresentToday = present,
                            ApprovedLeaveCount = approved,
                            PendingLeaveCount = pending,
                            TotalLeaveCount = deptLeaves.Count,
                            LeaveRate = totalEmp == 0 ? 0 : (double)approved / totalEmp,
                            WorkingRate = totalEmp == 0 ? 0 : (double)present / totalEmp
                        };
                    })
                    .ToList();

                // 3. THÊM DÒNG TỔNG CÔNG TY
                if (includeCompanyTotal)
                {
                    grouped.Add(new LeaveStatisticsViewModel
                    {
                        DepartmentId = "ALL",
                        DepartmentName = "FCC (Vietnam)",
                        TotalEmployees = employeesRaw.Count,
                        TotalPresentToday = presentTodayRaw.Count,
                        ApprovedLeaveCount = leavesRaw.Count(x => LeaveStatusHelper.IsApproved(x.RequestStatus)),
                        TotalLeaveCount = leavesRaw.Count
                    });
                }

                Logger.LogInfoIf(Debug, "[STAT] GetLeaveStatistics DONE: {DeptCount}", grouped.Count);

                return grouped;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[STAT] GetLeaveStatistics ERROR");
                throw; // Ném lỗi để DashboardService bắt được và trả về InternalError
            }
        }
        // ================= ABSENCE WARNING (MỚI) =================
        public async Task<AbsenceWarningDto> GetAbsenceWarningAsync(string deptCode, CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[STAT] Tính cảnh báo vắng mặt cho phòng: {Dept}", deptCode);
                var today = DateOnly.FromDateTime(DateTime.Today);

                // 1. Lấy tổng số nhân viên trong phòng ban
                var totalStaff = await _db.F03employees
                    .CountAsync(x => x.DeptCode == deptCode && x.IsActive == true, ct);

                // 2. Lấy số người đang có mặt thực tế (Từ View SQL đã sửa)
                var presentCount = await _db.VwCurrentlyPresentEmployees
                    .Where(x => x.DeptCode == deptCode && x.Date == today)
                    .Select(x => x.EmployeeId)
                    .Distinct()
                    .CountAsync(ct);

                // 3. Lấy số đơn đang chờ duyệt trong ngày hôm nay/mai (Cảnh báo sớm)
                // Lưu ý: Tùy vào cách Sinin lưu ngày, ở đây mình giả định check cho ngày hiện tại
                var pendingCount = await _db.VF03leaveDays
                    .CountAsync(x => x.DeptCode == deptCode
                                && x.RegisterDate == DateTime.Today
                                && x.RequestStatus == LeaveStatus.Pending, ct); // 0 thường là Pending

                // 4. Tính toán tỷ lệ
                int currentAbsent = totalStaff - presentCount;
                double absenceRate = totalStaff == 0 ? 0 : (double)currentAbsent / totalStaff * 100;

                // Ngưỡng mặc định là 10%, Sinin có thể lấy từ bảng F03Department nếu đã thêm cột Threshold
                double threshold = 10.0;

                var result = new AbsenceWarningDto
                {
                    DeptCode = deptCode,
                    TotalStaff = totalStaff,
                    CurrentAbsent = currentAbsent,
                    PendingRequests = pendingCount,
                    AbsenceRate = Math.Round(absenceRate, 1),
                    Threshold = threshold,
                    WarningMessage = absenceRate >= threshold
                        ? $"Cảnh báo: Tỷ lệ vắng mặt bộ phận hiện tại ({absenceRate:F1}%) đã vượt ngưỡng an toàn!"
                        : string.Empty
                };

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[STAT] GetAbsenceWarning ERROR cho Dept: {Dept}", deptCode);
                return new AbsenceWarningDto { DeptCode = deptCode }; // Trả về object trống để không làm hỏng Dashboard
            }
        }
        // ================= WIDGETS CHO HR/DASHBOARD =================
        public async Task<List<WidgetCounterDto>> GetCompanyDashboardWidgetsAsync(CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[STAT] Bắt đầu tổng hợp Dashboard Widgets toàn công ty");

                // Lấy các dữ liệu cơ bản
                var totalEmployees = await GetTotalEmployeesAsync(ct);
                var presentToday = await GetPresentTodayAsync(ct);

                // Lấy tổng số đơn đang chờ duyệt (toàn công ty)
                var totalPending = await _db.VF03leaveDays
                    .CountAsync(x => x.RequestStatus == LeaveStatus.Pending, ct); // Giả định 0 là Pending

                // Lấy số người nghỉ hôm nay (Phê duyệt rồi và ngày nghỉ là hôm nay)
                var today = DateTime.Today;
                var onLeaveToday = await _db.VF03leaveDays
                    .CountAsync(x => x.RegisterDate == today && x.RequestStatus == LeaveStatus.Approved, ct); // Giả định 2 là Approved

                var widgets = new List<WidgetCounterDto>
{
    new() {
        Title = "Tổng Nhân Viên",
        Value = totalEmployees.ToString(),
        Icon = "People", // Chỉ truyền tên
        Color = "Primary",
        Link = "/admin/employees"
    },
    new() {
        Title = "Hiện Diện",
        Value = presentToday.ToString(),
        Icon = "DirectionsRun",
        Color = "Success",
        Link = "/reports/attendance"
    },
    new() {
        Title = "Đang Nghỉ Phép",
        Value = onLeaveToday.ToString(),
        Icon = "EventBusy",
        Color = "Info",
        Link = "/reports/leave-today"
    },
    new() {
        Title = "Đơn Chờ Duyệt",
        Value = totalPending.ToString(),
        Icon = "PendingActions",
        Color = "Warning",
        Link = "/leave/approvals"
    }
};

                return widgets;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[STAT] GetCompanyDashboardWidgets ERROR");
                return new List<WidgetCounterDto>(); // Trả về list rỗng để tránh lỗi giao diện
            }
        }
    }
}
