using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Interfaces.Leaves;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Contract.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.API.Services.Leaves
{
    public class LeaveQueryService : ILeaveQueryService
    {
        private readonly FVNWEBAPPContext _db;

        public LeaveQueryService(FVNWEBAPPContext db)
        {
            _db = db;
        }

        // ================= COMBINED DATA (FIXED THREAD SAFETY) =================
        public async Task<CombinedHolidaysViewModel> GetCombinedDataAsync(
            string empCode,
            string depart,
            string cvCode,
            int year,
            CancellationToken ct = default)
        {
            // Thay vì chạy song song gây crash DbContext, ta gọi tuần tự an toàn
            var userLevel = await GetUserLevelAsync(empCode, ct);
            var leaveTypes = await GetLeaveTypesAsync(ct);
            var phepTon = await GetPhepTonAsync(empCode, ct);
            var workYears = await GetWorkYearsAsync(ct);
            var companyHolidays = await GetCompanyHolidaysAsync(year, ct);
            var holidaysNotCountLeave = await GetHolidayNotCountAsync(ct);
            var leaveDays = await GetLeaveDaysAsync(empCode, year, ct);

            var model = new CombinedHolidaysViewModel
            {
                UserLevel = userLevel,
                LeaveDays = leaveDays,
                LeaveTypes = leaveTypes,
                PhepTon = phepTon,
                WorkYears = workYears,
                CompanyHolidays = companyHolidays,
                HolidaysNotCountLeave = holidaysNotCountLeave,
                LeaveForm = new CreateLeaveRequestModel
                {
                    EmployeeCode = empCode,
                    Details = new List<CreateLeaveDetailModel>()
                }
            };

            model.LVA1 = await BuildApproverAsync(userLevel + 1, depart, cvCode, ct);
            model.LVA2 = await BuildApproverAsync(userLevel + 2, depart, cvCode, ct);
            model.LVA3 = await BuildApproverAsync(userLevel + 3, depart, cvCode, ct);

            return model;
        }

        // ================= PENDING SUMMARY (OPTIMIZED) =================
        public async Task<List<PendingApprovalGroup>> GetPendingSummaryAsync(
            string employeeCode,
            CancellationToken ct = default)
        {
            var email = await ResolveApproverEmailAsync(employeeCode, ct);
            if (string.IsNullOrEmpty(email)) return new();

            var query = _db.VF03leaveDays
                .AsNoTracking()
                .Where(x => x.IsActive == true &&
                            x.RequestStatus != LeaveStatus.Approved &&
                            x.RequestStatus != LeaveStatus.Rejected &&
                            x.RequestStatus != LeaveStatus.Cancel);

            // Gọi tuần tự để tránh lỗi đa luồng kết nối DB
            var countL1 = await query.CountAsync(x => x.Level1ApproveEmail == email && x.Level1IsApprove != true, ct);
            var countL2 = await query.CountAsync(x => x.Level2ApproveEmail == email && x.Level1IsApprove == true && x.Level2IsApprove != true, ct);
            var countL3 = await query.CountAsync(x => x.Level3ApproveEmail == email && x.Level2IsApprove == true && x.Level3IsApprove != true, ct);

            var result = new List<PendingApprovalGroup>
        {
            new() { ApproverLevel = 1, Count = countL1 },
            new() { ApproverLevel = 2, Count = countL2 },
            new() { ApproverLevel = 3, Count = countL3 }
        };

            return result.Where(x => x.Count > 0).ToList();
        }

        // ================= PENDING DETAILS =================
        public async Task<List<PendingApprovalGroup>> GetPendingDetailsAsync(
    string employeeCode,
    CancellationToken ct = default)
        {
            // ✅ Dùng cùng cách lấy email như Dashboard
            var email = await ResolveApproverEmailAsync(employeeCode, ct);

            if (string.IsNullOrEmpty(email)) return new();

            // ✅ Dùng cùng điều kiện như Dashboard
            var allPending = await _db.VF03leaveDays
                .AsNoTracking()
                .Where(x =>
                    x.IsActive == true &&
                    //x.RequestStatus != LeaveStatus.Approved &&   // ✅ THÊM
                    //x.RequestStatus != LeaveStatus.Rejected &&   // ✅ THÊM
                    //x.RequestStatus != LeaveStatus.Cancel &&     // ✅ THÊM
                    (
                        (x.Level1ApproveEmail == email && x.Level1IsApprove != true) ||
                        (x.Level2ApproveEmail == email && x.Level1IsApprove == true && x.Level2IsApprove != true) ||
                        (x.Level3ApproveEmail == email && x.Level2IsApprove == true && x.Level3IsApprove != true)
                    ))
                .ToListAsync(ct);

            // ✅ Phân nhóm theo cấp duyệt
            var lv1 = allPending
                .Where(x => x.Level1ApproveEmail == email && x.Level1IsApprove != true)
                .Select(MapToViewModel).ToList();

            var lv2 = allPending
                .Where(x => x.Level2ApproveEmail == email && x.Level1IsApprove == true && x.Level2IsApprove != true)
                .Select(MapToViewModel).ToList();

            var lv3 = allPending
                .Where(x => x.Level3ApproveEmail == email && x.Level2IsApprove == true && x.Level3IsApprove != true)
                .Select(MapToViewModel).ToList();

            var result = new List<PendingApprovalGroup>();

            if (lv1.Any())
                result.Add(new PendingApprovalGroup
                {
                    ApproverLevel = 1,
                    Count = lv1.Count,
                    Requests = lv1
                });

            if (lv2.Any())
                result.Add(new PendingApprovalGroup
                {
                    ApproverLevel = 2,
                    Count = lv2.Count,
                    Requests = lv2
                });

            if (lv3.Any())
                result.Add(new PendingApprovalGroup
                {
                    ApproverLevel = 3,
                    Count = lv3.Count,
                    Requests = lv3
                });

            return result;
        }

        // ================= SUPPORT =================
        private async Task<List<VF03leaveDay>> GetPendingBaseQueryAsync(string email, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(email)) return new();

            return await _db.VF03leaveDays
                .AsNoTracking()
                .Where(x => x.IsActive == true &&
                            x.RequestStatus != LeaveStatus.Approved &&
                            x.RequestStatus != LeaveStatus.Rejected &&
                            x.RequestStatus != LeaveStatus.Cancel &&
                            (x.Level1ApproveEmail == email || x.Level2ApproveEmail == email || x.Level3ApproveEmail == email))
                .ToListAsync(ct);
        }

        private async Task<List<VF03leaveDaysApprover>> GetApproverLevelAsync(int level, string deptCode, CancellationToken ct)
        {
            var baseQuery = _db.VF03leaveDaysApprovers
                .AsNoTracking()
                .Where(x => x.ApproveLevel == level);

            var list = await baseQuery
                .Where(x => x.DeptCode == deptCode)
                .ToListAsync(ct);

            return list.Any()
                ? list
                : await baseQuery
                    .Where(x => string.IsNullOrEmpty(x.DeptCode) || x.DeptCode == "ALL")
                    .ToListAsync(ct);
        }

        private async Task<int> GetUserLevelAsync(string empCode, CancellationToken ct)
        {
            return await _db.F03leaveDaysApprovers
                .Where(x => x.ApproveLevelCode == empCode)
                .Select(x => x.ApproveLevel)
                .FirstOrDefaultAsync(ct);
        }

        private async Task<List<VF03phepTon>> GetPhepTonAsync(string empCode, CancellationToken ct)
            => await _db.VF03phepTons
                .AsNoTracking()
                .Where(x => x.EmployeeCode == empCode)
                .ToListAsync(ct);

        private async Task<List<F03leaveType>> GetLeaveTypesAsync(CancellationToken ct)
            => await _db.F03leaveTypes
                .AsNoTracking()
                .Where(x => x.IsActive == true)
                .ToListAsync(ct);

        private async Task<List<F03workYear>> GetWorkYearsAsync(CancellationToken ct)
            => await _db.F03workYears
                .AsNoTracking()
                .OrderByDescending(x => x.WorkYear)
                .ToListAsync(ct);

        private async Task<string> ResolveApproverEmailAsync(
    string employeeCode, CancellationToken ct)
        {
            // Tìm trong bảng approver trước
            var email = await _db.F03leaveDaysApprovers
                .Where(x => x.ApproveLevelCode == employeeCode && x.IsActive)
                .Select(x => x.ApproveLevelEmail)
                .FirstOrDefaultAsync(ct);

            if (!string.IsNullOrEmpty(email)) return email;

            // ✅ Fallback: lấy email từ bảng employee
            return await _db.VF03employees
                .Where(x => x.EmployeeCode == employeeCode)
                .Select(x => x.EmailAddress)
                .FirstOrDefaultAsync(ct) ?? "";
        }

        private async Task<List<HolidayViewModel>> GetCompanyHolidaysAsync(int year, CancellationToken ct)
        {
            var data = await _db.CompanyHolidays
                .AsNoTracking()
                .Where(x => x.HolidayDate != null && x.HolidayDate.Value.Year == year)
                .ToListAsync(ct);

            return data.Select(x => new HolidayViewModel
            {
                Id = x.Id.ToString(),
                Title = "Ngày nghỉ",
                Start = x.HolidayDate?.ToString("yyyy-MM-dd"),
                End = x.HolidayDate?.ToString("yyyy-MM-dd"),
                Status = "Holiday",
                ExtendedProps = new ExtendedProps
                {
                    description = x.Description ?? "",
                    TinhPhep = x.TinhPhep ?? 0
                },
                ClassNames = new List<string> { "holiday-event" }
            }).ToList();
        }

        // FIXED: Sửa lỗi dùng .ToString() trong biểu thức Linq to SQL
        private async Task<List<string>> GetHolidayNotCountAsync(CancellationToken ct)
        {
            var dates = await _db.CompanyHolidays
                .AsNoTracking()
                .Where(x => x.TinhPhep == 0 && x.HolidayDate != null)
                .Select(x => x.HolidayDate) // Chỉ lấy Date thô từ SQL lên
                .ToListAsync(ct);

            // Đưa lên RAM rồi mới đổi định dạng chuỗi chu đáo
            return dates.Select(d => d!.Value.ToString("yyyy-MM-dd")).ToList();
        }

        private async Task<List<HolidayViewModel>> GetLeaveDaysAsync(
    string empCode,
    int? year,
    CancellationToken ct)
        {
            var query = _db.VF03leaveDayDetails
                .AsNoTracking()
                .Where(x => x.EmployeeCode == empCode);

            if (year.HasValue)
                query = query.Where(x => x.WorkYear == year.Value);

            var data = await query.ToListAsync(ct);

            return data.Select(d => new HolidayViewModel
            {
                Id = d.DetailId.ToString(),           // ✅ DetailId — cancel từng ngày
                Inforregister = d.LeaveId.ToString(), // ✅ LeaveId  — xem/cancel cả đơn

                Title = d.LeaveTypeName ?? "Nghỉ phép",
                Start = d.LeaveDate.ToString("yyyy-MM-dd"),
                End = d.LeaveDate.ToString("yyyy-MM-dd"),
                Status = d.RequestStatus,
                ExtendedProps = new ExtendedProps
                {
                    APstatus = d.RequestStatus ?? "",
                    TinhPhep = d.TinhPhep ? 1 : 0,
                    totalDay = d.DayValue,
                    level1ApprovedBy = d.Level1ApproveName ?? "",
                    level2ApprovedBy = d.Level2ApproveName ?? "",
                },
                ClassNames = new List<string> { GetStatusClass(d.RequestStatus) }
            }).ToList();
        }
        private string GetStatusClass(string? status) => status switch
        {
            "Approved" => "event-approved",
            "Rejected" => "event-rejected",
            "ApprovedLv1" => "event-lv1",
            "ApprovedLv2" => "event-lv2",
            _ => "event-pending"
        };

        public async Task<List<WidgetCounterDto>> GetDashboardWidgetsAsync(string employeeCode, string deptCode, CancellationToken ct = default)
        {
            var email = await ResolveApproverEmailAsync(employeeCode, ct);
            var today = DateTime.Today;

            var pendingCount = await _db.VF03leaveDays.CountAsync(x =>
                x.IsActive == true &&
                ((x.Level1ApproveEmail == email && x.Level1IsApprove != true) ||
                 (x.Level2ApproveEmail == email && x.Level1IsApprove == true && x.Level2IsApprove != true) ||
                 (x.Level3ApproveEmail == email && x.Level2IsApprove == true && x.Level3IsApprove != true)), ct);

            var absentToday = await _db.VF03leaveDays.CountAsync(x =>
                x.DeptCode == deptCode &&
                x.RequestStatus == LeaveStatus.Approved &&
                x.StartDate <= today && x.EndDate >= today, ct);

            return new List<WidgetCounterDto>
        {
            new() { Title = "Đơn chờ duyệt", Value = pendingCount.ToString(), Icon = "Icons.Material.Filled.HourglassEmpty", Color = "warning", Link = "/approve/list" },
            new() { Title = "Vắng mặt hôm nay", Value = absentToday.ToString(), Icon = "Icons.Material.Filled.People", Color = "info", Link = "/leave/department-status" },
            new() { Title = "Cảnh báo", Value = absentToday > 10 ? "Cao" : "Bình thường", Icon = "Icons.Material.Filled.Warning", Color = absentToday > 10 ? "error" : "success" }
        };
        }

        public async Task<List<RecentLeaveRequestDto>> GetRecentHistoryAsync(string employeeCode, int limit = 5, CancellationToken ct = default)
        {
            return await _db.F03leaveDays
                .AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode && x.IsActive == true)
                .OrderByDescending(x => x.CreatedAt)
                .Take(limit)
                .Select(x => new RecentLeaveRequestDto
                {
                    RequestId = x.Id,
                    FromDate = x.StartDate,
                    ToDate = x.EndDate,
                    Status = x.RequestStatus ?? LeaveStatus.Pending,
                    ApproverName = x.Level1ApproveName ?? "N/A"
                })
                .ToListAsync(ct);
        }

        public async Task<LeaveBalanceDto> GetSimpleBalanceAsync(string employeeCode, int year, CancellationToken ct = default)
        {
            var phep = await _db.VF03phepTons
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.EmployeeCode == employeeCode && x.WorkYear == year, ct);

            return new LeaveBalanceDto
            {
                Entitled = phep?.TongPhep ?? 0,
                Used = phep?.SoNgayNghiPhep ?? 0,
                Remaining = phep?.PhepTon ?? 0,
                Year = year
            };
        }

        private async Task<List<VF03leaveDaysApprover>> BuildApproverAsync(int level, string deptCode, string cvCode, CancellationToken ct)
        {
            if (level <= 0 || level > 4) return new();
            if (cvCode != "0003" && level < 3) level = Math.Max(level, 3);
            return await GetApproverLevelAsync(level, deptCode, ct);
        }

        private LeaveDaysViewModel MapToViewModel(VF03leaveDay x) => new()
        {
            Id = x.Id ?? 0,
            EmployeeCode = x.EmployeeCode,
            EmployeeName = x.EmployeeName,
            StartDate = x.StartDate ?? DateTime.Now,
            EndDate = x.EndDate ?? DateTime.Now,
            TotalDay = x.TotalDay ?? 0,
            RequestStatus = x.RequestStatus,
            DeptCode = x.DeptCode,
            LeaveReason = x.LeaveReason ?? "",
            Level1IsApprove = x.Level1IsApprove ?? false,
            Level2IsApprove = x.Level2IsApprove ?? false,
            Level3IsApprove = x.Level3IsApprove ?? false,
        };

        private HolidayViewModel MapHoliday(VF03leaveDay x) => new()
        {
            Id = x.Id?.ToString() ?? Guid.NewGuid().ToString(),
            Title = x.EmployeeName ?? "",
            Start = x.StartDate?.ToString("yyyy-MM-dd"),
            End = x.EndDate?.ToString("yyyy-MM-dd"),
            Status = x.RequestStatus ?? "",
            ExtendedProps = new ExtendedProps
            {
                registerdate = x.RegisterDate,
                daylev = decimal.TryParse(x.LeaveDay, out var d) ? d : 0,
                totalDay = x.TotalDay ?? 0,
                description = x.LeaveReason ?? "",
                APstatus = x.RequestStatus ?? "",
                Level1ApproveEmail = x.Level1ApproveEmail,
                level1ApprovedBy = x.Level1ApproveName,
                level1ApprovedDate = x.Level1ApproveTime
            }
        };
    }
}
