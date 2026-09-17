using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Application.Interfaces.Statics;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Core.Utils; // TimeRange
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Statics
{
    /// <summary>
    /// LƯU Ý QUAN TRỌNG (không được bỏ qua khi sửa file này):
    /// F03LeaveDay.StartDate / EndDate là 2 property [NotMapped] (chỉ để tương thích code cũ),
    /// chúng chỉ redirect sang StartTime / EndTime chứ KHÔNG có cột trong DB.
    ///
    /// => TUYỆT ĐỐI KHÔNG dùng x.StartDate / x.EndDate bên trong bất kỳ biểu thức LINQ nào
    ///    sẽ được EF Core dịch sang SQL (Where/OrderBy/Join/Select trên IQueryable).
    ///    Luôn dùng x.StartTime / x.EndTime thay thế.
    ///
    /// StartDate/EndDate chỉ an toàn khi dùng SAU khi đã ToList()/AsEnumerable() (in-memory),
    /// hoặc trong code thuần C# không liên quan tới EF Core.
    /// </summary>
    public class StatisticsService : BaseService<StatisticsService>, IStatisticsService
    {
        private readonly IUnitOfWork _uow;

        public StatisticsService(
            IUnitOfWork uow,
            ILogger<StatisticsService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _uow = uow;
        }

        // ================= TOTAL EMPLOYEE =================
        public async Task<int> GetTotalEmployeesAsync(CancellationToken ct = default)
            => await _uow.Repository<F03Employee>().Query()
                .CountAsync(x => x.IsActive == true, ct);

        // ================= PRESENT TODAY =================
        public async Task<int> GetPresentTodayAsync(CancellationToken ct = default)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return await _uow.Repository<VwCurrentlyPresentEmployee>().Query()
                .Where(x => x.Date == today)
                .Select(x => x.EmployeeId)
                .Distinct()
                .CountAsync(ct);
        }

        // ================= HOLIDAY COUNT =================
        public async Task<int> GetHolidayCountAsync(int year, CancellationToken ct = default)
            => await _uow.Repository<F03CompanyHoliday>().Query()
                .CountAsync(x => x.Year == year, ct);

        // ================= WORK YEAR =================
        public async Task<List<WorkYearDto>> GetWorkYearStatsAsync(CancellationToken ct = default)
        {
            return await _uow.Repository<F03WorkYear>().Query()
                .OrderByDescending(x => x.WorkYear)
                .Select(x => new WorkYearDto
                {
                    Id = x.Id,
                    Year = x.WorkYear,
                    YearName = $"Năm {x.WorkYear}",
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    IsActive = x.IsActive == true
                })
                .ToListAsync(ct);
        }

        // ================= DEPARTMENT (1 phòng - có chi tiết đơn) =================
        public async Task<LeaveStatisticsDto> GetDepartmentStatisticsAsync(
            string deptCode, CancellationToken ct = default)
        {
            var summary = (await GetLeaveStatisticsAsync(false, ct))
                .FirstOrDefault(x => x.DepartmentId == deptCode) ?? new LeaveStatisticsDto
                {
                    DepartmentId = deptCode
                };

            summary.EmployeeLeaves = await LoadEmployeeLeavesForDeptAsync(deptCode, ct);
            summary.Departments = await _uow.Repository<F03Department>().Query()
                .Where(d => d.IsActive)
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    DeptCode = d.DeptCode,
                    DeptName = d.DeptName,
                    IsActive = d.IsActive,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync(ct);

            return summary;
        }

        // Helper riêng: load đơn nghỉ + chi tiết cho 1 phòng ban
        private async Task<List<LeaveRequestDto>> LoadEmployeeLeavesForDeptAsync(
            string deptCode, CancellationToken ct)
        {
            var empCodes = await _uow.Repository<F03Employee>().Query()
                .Where(e => e.DeptCode == deptCode && e.IsActive == true)
                .Select(e => e.EmployeeCode)
                .ToListAsync(ct);

            if (empCodes.Count == 0) return new();

            var leaves = await _uow.Repository<F03LeaveDay>().Query()
                .Where(l => l.IsActive == true && empCodes.Contains(l.EmployeeCode))
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync(ct);

            var leaveIds = leaves.Select(l => l.Id).ToList();

            var details = await _uow.Repository<F03LeaveDayDetail>().Query()
                .Where(d => leaveIds.Contains(d.LeaveDaysId))
                .ToListAsync(ct);

            var empDict = await _uow.Repository<F03Employee>().Query()
                .Where(e => empCodes.Contains(e.EmployeeCode))
                .ToDictionaryAsync(e => e.EmployeeCode, e => e, ct);

            return leaves.Select(l =>
            {
                empDict.TryGetValue(l.EmployeeCode, out var emp);

                return new LeaveRequestDto
                {
                    Id = l.Id,
                    RegisterDate = l.CreatedAt,
                    RequesterCode = l.EmployeeCode,
                    RequesterName = emp?.EmployeeName ?? string.Empty,
                    DeptCode = emp?.DeptCode ?? string.Empty,
                    DeptName = string.Empty, // fill nếu cần join F03Department
                    RequestStatus = l.RequestStatus,
                    WorkYear = l.WorkYear,
                    // l ở đây đã materialize (List<F03LeaveDay> trong RAM) nên dùng
                    // StartDate/EndDate (NotMapped) ở bước mapping DTO là AN TOÀN.
                    Details = details
                        .Where(d => d.LeaveDaysId == l.Id)
                        .Select(d => new LeaveRequestDetailDto
                        {
                            LeaveDate = d.LeaveDate,
                            LeaveTypeCode = d.LeaveTypeCode,
                            LeaveTypeName = d.LeaveTypeName,
                            IsCountedAsLeave = d.IsCountedAsLeave,
                            IsHalfDay = d.IsHalfDay,
                            HalfDayOption = d.HalfDayOption,
                            DayValue = d.DayValue
                        })
                        .ToList()
                };
            }).ToList();
        }

        // ================= TIME RANGE =================
        public async Task<List<LeaveStatisticsDto>> GetStatisticsByTimeRangeAsync(
            string[]? departments, TimeRange timeRange, CancellationToken ct = default)
        {
            var today = DateTime.Today;
            var fromDate = timeRange switch
            {
                TimeRange.Week => today.AddDays(-7),
                TimeRange.Month => today.AddMonths(-1),
                TimeRange.Year => today.AddYears(-1),
                _ => today
            };

            var query = _uow.Repository<VF03LeaveRequest>().Query()
                .Where(x => x.IsActive == true &&
                            x.RegisterDate >= fromDate && x.RegisterDate <= today);

            if (departments != null && departments.Length > 0)
                query = query.Where(x => departments.Contains(x.DeptCode));

            return await query
                .GroupBy(x => new { x.DeptCode, x.DeptName })
                .Select(g => new LeaveStatisticsDto
                {
                    DepartmentId = g.Key.DeptCode ?? "",
                    DepartmentName = g.Key.DeptName ?? "",
                    ApprovedLeaveCount = g.Count(x => x.RequestStatus == ApprovalStatus.Approved),
                    PendingLeaveCount = g.Count(x =>
                        x.RequestStatus == ApprovalStatus.Pending ||
                        x.RequestStatus == ApprovalStatus.InProgress),
                    TotalLeaveCount = g.Count()
                })
                .ToListAsync(ct);
        }

        // ================= LEAVE STATISTICS (toàn công ty, KHÔNG load EmployeeLeaves) =================
        public async Task<List<LeaveStatisticsDto>> GetLeaveStatisticsAsync(
            bool includeCompanyTotal = false, CancellationToken ct = default)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var employeesByDept = await _uow.Repository<F03Employee>().Query()
                .Where(x => x.IsActive == true)
                .GroupBy(x => x.DeptCode)
                .Select(g => new { DeptCode = g.Key, Total = g.Count() })
                .ToListAsync(ct);

            var deptNames = await _uow.Repository<F03Department>().Query()
                .ToDictionaryAsync(x => x.DeptCode, x => x.DeptName, ct);

            var leaveStatsByDept = await _uow.Repository<F03LeaveDay>().Query()
                .Where(l => l.IsActive == true)
                .Join(
                    _uow.Repository<F03Employee>().Query().Where(e => e.IsActive == true),
                    l => l.EmployeeCode, e => e.EmployeeCode,
                    (l, e) => new { e.DeptCode, l.RequestStatus })
                .GroupBy(x => x.DeptCode)
                .Select(g => new
                {
                    DeptCode = g.Key,
                    Approved = g.Count(x => x.RequestStatus == ApprovalStatus.Approved),
                    Pending = g.Count(x =>
                        x.RequestStatus == ApprovalStatus.Pending ||
                        x.RequestStatus == ApprovalStatus.InProgress),
                    Total = g.Count()
                })
                .ToListAsync(ct);

            var presentByDept = await _uow.Repository<VwCurrentlyPresentEmployee>().Query()
                .Where(x => x.Date == today)
                .GroupBy(x => x.DeptCode)
                .Select(g => new { DeptCode = g.Key, Count = g.Select(x => x.EmployeeId).Distinct().Count() })
                .ToListAsync(ct);

            var leaveDict = leaveStatsByDept.ToDictionary(x => x.DeptCode ?? "", x => x);
            var presentDict = presentByDept.ToDictionary(x => x.DeptCode ?? "", x => x.Count);

            var result = employeesByDept.Select(e =>
            {
                var deptCode = e.DeptCode ?? "";
                leaveDict.TryGetValue(deptCode, out var leave);
                var present = presentDict.GetValueOrDefault(deptCode, 0);

                return new LeaveStatisticsDto
                {
                    DepartmentId = deptCode,
                    DepartmentName = deptNames.GetValueOrDefault(deptCode, deptCode),
                    TotalEmployees = e.Total,
                    PresentEmployeesCount = present,
                    ApprovedLeaveCount = leave?.Approved ?? 0,
                    PendingLeaveCount = leave?.Pending ?? 0,
                    TotalLeaveCount = leave?.Total ?? 0,
                    LeaveRate = e.Total == 0 ? 0 : (double)(leave?.Approved ?? 0) / e.Total,
                    WorkingRate = e.Total == 0 ? 0 : (double)present / e.Total
                    // EmployeeLeaves / Departments: KHÔNG populate ở đây theo chốt thiết kế
                };
            }).ToList();

            if (includeCompanyTotal)
            {
                result.Add(new LeaveStatisticsDto
                {
                    DepartmentId = "ALL",
                    DepartmentName = "FCC (Vietnam)",
                    TotalEmployees = employeesByDept.Sum(x => x.Total),
                    PresentEmployeesCount = presentByDept.Sum(x => x.Count),
                    ApprovedLeaveCount = leaveStatsByDept.Sum(x => x.Approved),
                    PendingLeaveCount = leaveStatsByDept.Sum(x => x.Pending),
                    TotalLeaveCount = leaveStatsByDept.Sum(x => x.Total)
                });
            }

            return result;
        }

        // ================= ABSENCE WARNING =================
        public async Task<AbsenceWarningDto> GetAbsenceWarningAsync(
            string deptCode, CancellationToken ct = default)
        {
            const double threshold = 10.0;
            var today = DateOnly.FromDateTime(DateTime.Today);
            var todayDate = DateTime.Today;
            var tomorrow = todayDate.AddDays(1);

            var totalStaff = await _uow.Repository<F03Employee>().Query()
                .CountAsync(x => x.DeptCode == deptCode && x.IsActive == true, ct);

            var presentCount = await _uow.Repository<VwCurrentlyPresentEmployee>().Query()
                .Where(x => x.DeptCode == deptCode && x.Date == today)
                .Select(x => x.EmployeeId)
                .Distinct()
                .CountAsync(ct);

            // 🔧 FIX: x.StartDate / x.EndDate là [NotMapped] trên F03LeaveDay -> không thể
            // dịch sang SQL. Dùng x.StartTime / x.EndTime (property thật, có mapping).
            var pendingCount = await _uow.Repository<F03LeaveDay>().Query()
                .Where(x =>
                    x.IsActive == true &&
                    (x.RequestStatus == ApprovalStatus.Pending || x.RequestStatus == ApprovalStatus.InProgress) &&
                    x.StartTime <= tomorrow &&
                    x.EndTime >= todayDate)
                .Join(
                    _uow.Repository<F03Employee>().Query()
                        .Where(e => e.DeptCode == deptCode && e.IsActive == true),
                    l => l.EmployeeCode, e => e.EmployeeCode,
                    (l, e) => l.Id)
                .CountAsync(ct);

            int currentAbsent = totalStaff - presentCount;
            double absenceRate = totalStaff == 0 ? 0 : (double)currentAbsent / totalStaff * 100;

            return new AbsenceWarningDto
            {
                DeptCode = deptCode,
                TotalStaff = totalStaff,
                CurrentAbsent = currentAbsent,
                PendingRequests = pendingCount,
                AbsenceRate = Math.Round(absenceRate, 1),
                Threshold = threshold,
                WarningMessage = absenceRate >= threshold
                    ? $"Cảnh báo: Tỷ lệ vắng mặt ({absenceRate:F1}%) vượt ngưỡng an toàn!"
                    : string.Empty
            };
        }

        // ================= DASHBOARD WIDGETS =================
        public Task<List<WidgetCounterDto>> GetCompanyDashboardWidgetsAsync(CancellationToken ct = default)
            => BuildWidgetsAsync(deptCode: null, totalTitle: "Tổng nhân viên", ct);

        public Task<List<WidgetCounterDto>> GetDeptDashboardWidgetsAsync(
            string deptCode, CancellationToken ct = default)
            => BuildWidgetsAsync(deptCode, totalTitle: "Nhân viên bộ phận", ct);

        private async Task<List<WidgetCounterDto>> BuildWidgetsAsync(
            string? deptCode, string totalTitle, CancellationToken ct)
        {
            var today = DateTime.Today;
            var todayOnly = DateOnly.FromDateTime(today);

            var empQuery = _uow.Repository<F03Employee>().Query().Where(x => x.IsActive == true);
            if (deptCode != null) empQuery = empQuery.Where(x => x.DeptCode == deptCode);
            var totalEmp = await empQuery.CountAsync(ct);

            var presentQuery = _uow.Repository<VwCurrentlyPresentEmployee>().Query()
                .Where(x => x.Date == todayOnly);
            if (deptCode != null) presentQuery = presentQuery.Where(x => x.DeptCode == deptCode);
            var presentToday = await presentQuery.Select(x => x.EmployeeId).Distinct().CountAsync(ct);

            var leaveBaseQuery = _uow.Repository<F03LeaveDay>().Query().Where(x => x.IsActive == true);
            if (deptCode != null)
            {
                leaveBaseQuery = leaveBaseQuery.Join(
                    _uow.Repository<F03Employee>().Query().Where(e => e.DeptCode == deptCode && e.IsActive == true),
                    l => l.EmployeeCode, e => e.EmployeeCode,
                    (l, e) => l);
            }

            // 🔧 FIX: tương tự trên - dùng StartTime/EndTime thay vì StartDate/EndDate.
            var onLeaveToday = await leaveBaseQuery
                .Where(x => x.RequestStatus == ApprovalStatus.Approved &&
                            x.StartTime <= today && x.EndTime >= today)
                .CountAsync(ct);

            var totalPending = await leaveBaseQuery
                .Where(x => x.RequestStatus == ApprovalStatus.Pending || x.RequestStatus == ApprovalStatus.InProgress)
                .CountAsync(ct);

            return new List<WidgetCounterDto>
            {
                new() { Title = totalTitle, Value = totalEmp.ToString(), Icon = "People", Color = "Primary", Link = "/admin/employees" },
                new() { Title = "Có mặt hôm nay", Value = presentToday.ToString(), Icon = "DirectionsRun", Color = "Success", Link = "/leave/department-status" },
                new() { Title = "Đang nghỉ phép", Value = onLeaveToday.ToString(), Icon = "EventBusy", Color = "Info", Link = "/leave/department-status" },
                new() { Title = "Đơn chờ duyệt", Value = totalPending.ToString(), Icon = "PendingActions", Color = "Warning", Link = "/approvals" }
            };
        }
    }
}