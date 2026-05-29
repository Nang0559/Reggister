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

        // ================= COMBINED DATA =================
        public async Task<CombinedHolidaysViewModel> GetCombinedDataAsync(
            string empCode,
            string depart,
            string cvCode,
            int? year,
            CancellationToken ct = default)
        {
            var userLevelTask = GetUserLevelAsync(empCode, ct);
            var leaveTypesTask = GetLeaveTypesAsync(ct);
            var phepTonTask = GetPhepTonAsync(empCode, ct);
            var workYearsTask = GetWorkYearsAsync(ct);
            var holidaysTask = GetCompanyHolidaysAsync(ct);
            var holidaysNotCountTask = GetHolidayNotCountAsync(ct);
            var leaveDaysTask = GetLeaveDaysAsync(empCode, year, ct);

            await Task.WhenAll(
                userLevelTask,
                leaveTypesTask,
                phepTonTask,
                workYearsTask,
                holidaysTask,
                holidaysNotCountTask,
                leaveDaysTask);

            var userLevel = await userLevelTask;

            var model = new CombinedHolidaysViewModel
            {
                UserLevel = userLevel,
                LeaveDays = await leaveDaysTask,
                LeaveTypes = await leaveTypesTask,
                PhepTon = await phepTonTask,
                WorkYears = await workYearsTask,
                CompanyHolidays = await holidaysTask,
                HolidaysNotCountLeave = await holidaysNotCountTask,
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

            // 1. Thực hiện Query đếm trực tiếp trên Database (Performance ++ )
            var query = _db.VF03leaveDays
                .AsNoTracking()
                .Where(x => x.IsActive == true &&
                            x.RequestStatus != LeaveStatus.Approved &&
                            x.RequestStatus != LeaveStatus.Rejected &&
                            x.RequestStatus != LeaveStatus.Cancel);

            // 2. Định nghĩa các điều kiện chờ duyệt (Logic trung tâm)
            var isL1Pending = query.Where(x => x.Level1ApproveEmail == email && x.Level1IsApprove != true);
            var isL2Pending = query.Where(x => x.Level2ApproveEmail == email && x.Level1IsApprove == true && x.Level2IsApprove != true);
            var isL3Pending = query.Where(x => x.Level3ApproveEmail == email && x.Level2IsApprove == true && x.Level3IsApprove != true);

            // 3. Chạy song song các lệnh Count để tối ưu thời gian phản hồi
            var countL1Task = isL1Pending.CountAsync(ct);
            var countL2Task = isL2Pending.CountAsync(ct);
            var countL3Task = isL3Pending.CountAsync(ct);

            await Task.WhenAll(countL1Task, countL2Task, countL3Task);

            // 4. Trả về kết quả (Chỉ trả về các Level có đơn để UI hiển thị gọn hơn)
            var result = new List<PendingApprovalGroup>
    {
        new() { ApproverLevel = 1, Count = await countL1Task },
        new() { ApproverLevel = 2, Count = await countL2Task },
        new() { ApproverLevel = 3, Count = await countL3Task }
    };

            return result.Where(x => x.Count > 0).ToList();
        }

        // ================= PENDING DETAILS =================
        public async Task<List<PendingApprovalGroup>> GetPendingDetailsAsync(
            string employeeCode,
            CancellationToken ct = default)
        {
            var email = await ResolveApproverEmailAsync(employeeCode, ct);
            if (string.IsNullOrEmpty(email)) return new();

            var data = await GetPendingBaseQueryAsync(email, ct);

            var result = new List<PendingApprovalGroup>
        {
            new()
            {
                ApproverLevel = 1,
                Requests = data
                    .Where(x => x.Level1ApproveEmail == email && x.Level1IsApprove != true)
                    .Select(MapToViewModel)
                    .ToList()
            },
            new()
            {
                ApproverLevel = 2,
                Requests = data
                    .Where(x => x.Level2ApproveEmail == email && x.Level1IsApprove == true && x.Level2IsApprove != true)
                    .Select(MapToViewModel)
                    .ToList()
            },
            new()
            {
                ApproverLevel = 3,
                Requests = data
                    .Where(x => x.Level3ApproveEmail == email && x.Level2IsApprove == true && x.Level3IsApprove != true)
                    .Select(MapToViewModel)
                    .ToList()
            }
        };

            foreach (var g in result)
                g.Count = g.Requests.Count;

            return result.Where(x => x.Count > 0).ToList();
        }

        // ================= SUPPORT =================
        private async Task<List<VF03leaveDay>> GetPendingBaseQueryAsync(
     string email,
     CancellationToken ct)
        {
            if (string.IsNullOrEmpty(email)) return new();

            return await _db.VF03leaveDays
                .AsNoTracking()
                .Where(x =>
                    x.IsActive == true &&
                    // Loại bỏ các đơn đã kết thúc bằng hằng số cho chính xác
                    x.RequestStatus != LeaveStatus.Approved &&
                    x.RequestStatus != LeaveStatus.Rejected &&
                    x.RequestStatus != LeaveStatus.Cancel &&
                    (
                        x.Level1ApproveEmail == email ||
                        x.Level2ApproveEmail == email ||
                        x.Level3ApproveEmail == email
                    ))
                .ToListAsync(ct);
        }

        private async Task<List<VF03leaveDaysApprover>> GetApproverLevelAsync(
            int level,
            string deptCode,
            CancellationToken ct)
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
                .Where(x => x.IsActive==true)
                .ToListAsync(ct);

        private async Task<List<F03workYear>> GetWorkYearsAsync(CancellationToken ct)
            => await _db.F03workYears
                .AsNoTracking()
                .OrderByDescending(x => x.WorkYear)
                .ToListAsync(ct);

        private async Task<string> ResolveApproverEmailAsync(string employeeCode, CancellationToken ct)
            => await _db.F03leaveDaysApprovers
                .Where(x => x.ApproveLevelCode == employeeCode)
                .Select(x => x.ApproveLevelEmail)
                .FirstOrDefaultAsync(ct) ?? "";

        private async Task<List<HolidayViewModel>> GetCompanyHolidaysAsync(CancellationToken ct)
        {
            var data = await _db.CompanyHolidays
                .AsNoTracking()
                .Where(x => x.HolidayDate != null)
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

        private async Task<List<string>> GetHolidayNotCountAsync(CancellationToken ct)
        {
            return await _db.CompanyHolidays
                .AsNoTracking()
                .Where(x => x.TinhPhep == 0 && x.HolidayDate != null)
                .Select(x => x.HolidayDate!.Value.ToString("yyyy-MM-dd"))
                .ToListAsync(ct);
        }

        private async Task<List<HolidayViewModel>> GetLeaveDaysAsync(
            string empCode,
            int? year,
            CancellationToken ct)
        {
            var query = _db.VF03leaveDays
                .AsNoTracking()
                .Where(x => x.EmployeeCode == empCode);

            if (year.HasValue)
                query = query.Where(x => x.StartDate.HasValue && x.StartDate.Value.Year == year.Value);

            var data = await query.ToListAsync(ct);

            return data.Select(MapHoliday).ToList();
        }
        // 1. Lấy dữ liệu cho các thẻ Widget ở Dashboard
        public async Task<List<WidgetCounterDto>> GetDashboardWidgetsAsync(
    string employeeCode,
    string deptCode,
    CancellationToken ct = default)
        {
            var email = await ResolveApproverEmailAsync(employeeCode, ct);
            var today = DateTime.Today;

            // 1. Lấy dữ liệu thô từ DB
            var pendingCount = await _db.VF03leaveDays.CountAsync(x =>
                x.IsActive == true &&
                (
                    (x.Level1ApproveEmail == email && x.Level1IsApprove != true) ||
                    (x.Level2ApproveEmail == email && x.Level1IsApprove == true && x.Level2IsApprove != true) ||
                    (x.Level3ApproveEmail == email && x.Level2IsApprove == true && x.Level3IsApprove != true)
                ), ct);

            var absentToday = await _db.VF03leaveDays.CountAsync(x =>
                x.DeptCode == deptCode &&
                x.RequestStatus == LeaveStatus.Approved &&
                x.StartDate <= today && x.EndDate >= today, ct);

            // 2. Map sang List DTO khớp với UI MudBlazor
            return new List<WidgetCounterDto>
    {
        new() {
            Title = "Đơn chờ duyệt",
            Value = pendingCount.ToString(),
            Icon = "Icons.Material.Filled.HourglassEmpty",
            Color = "warning",
            Link = "/leave/approvals"
        },
        new() {
            Title = "Vắng mặt hôm nay",
            Value = absentToday.ToString(),
            Icon = "Icons.Material.Filled.People",
            Color = "info",
            Link = "/leave/department-status"
        },
        new() {
            Title = "Cảnh báo",
            Value = absentToday > 10 ? "Cao" : "Bình thường",
            Icon = "Icons.Material.Filled.Warning",
            Color = absentToday > 10 ? "error" : "success"
        }
    };
        }

        // 2. Lấy danh sách đơn gần đây (Dùng RecentLeaveRequestDto - Rất nhẹ)
        public async Task<List<RecentLeaveRequestDto>> GetRecentHistoryAsync(
            string employeeCode,
            int limit = 5,
            CancellationToken ct = default)
        {
            return await _db.F03leaveDays
                .AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode && x.IsActive == true)
                .OrderByDescending(x => x.CreatedAt)
                .Take(limit)
                .Select(x => new RecentLeaveRequestDto
                {
                    RequestId = x.Id,
                    // Dùng DateOnly hoặc DateTime tùy vào DTO bạn định nghĩa
                    FromDate = x.StartDate ,
                    ToDate = x.EndDate,
                    Status = x.RequestStatus ?? LeaveStatus.Pending,
                    ApproverName = x.Level1ApproveName ?? "N/A"
                })
                .ToListAsync(ct);
        }

        // 3. Lấy số dư phép nhanh (Dùng LeaveBalanceDto)
        public async Task<LeaveBalanceDto> GetSimpleBalanceAsync(
            string employeeCode,
            int year,
            CancellationToken ct = default)
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
        private async Task<List<VF03leaveDaysApprover>> BuildApproverAsync(
            int level,
            string deptCode,
            string cvCode,
            CancellationToken ct)
        {
            if (level <= 0 || level > 4) return new();

            if (cvCode != "0003" && level < 3)
                level = Math.Max(level, 3);

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
            DeptCode = x.DeptCode
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
