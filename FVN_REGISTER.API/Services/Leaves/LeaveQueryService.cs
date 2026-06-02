using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Interfaces.Leaves;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Contract.ViewModels;
using Microsoft.EntityFrameworkCore;




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
                int year,
                CancellationToken ct = default)
            {
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

                // ✅ FIX: truyền thẳng level 1, 2, 3 — KHÔNG cộng userLevel vào
                // cvCode chỉ dùng để quyết định BẮT BUỘC chọn đủ 3 cấp hay không (validator),
                // không phải để dịch chuyển level.
                model.LVA1 = await BuildApproverAsync(1, depart, ct);
                model.LVA2 = await BuildApproverAsync(2, depart, ct);
                model.LVA3 = await BuildApproverAsync(3, depart, ct);

                return model;
            }

            // ================= PENDING SUMMARY =================
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
                var email = await ResolveApproverEmailAsync(employeeCode, ct);
                if (string.IsNullOrEmpty(email)) return new();

                var allPending = await _db.VF03leaveDays
                    .AsNoTracking()
                    .Where(x =>
                        x.IsActive == true &&
                        x.RequestStatus != LeaveStatus.Approved &&
                        x.RequestStatus != LeaveStatus.Rejected &&
                        x.RequestStatus != LeaveStatus.Cancel &&
                        (
                            (x.Level1ApproveEmail == email && x.Level1IsApprove != true) ||
                            (x.Level2ApproveEmail == email && x.Level1IsApprove == true && x.Level2IsApprove != true) ||
                            (x.Level3ApproveEmail == email && x.Level2IsApprove == true && x.Level3IsApprove != true)
                        ))
                    .ToListAsync(ct);

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

                if (lv1.Any()) result.Add(new PendingApprovalGroup { ApproverLevel = 1, Count = lv1.Count, Requests = lv1 });
                if (lv2.Any()) result.Add(new PendingApprovalGroup { ApproverLevel = 2, Count = lv2.Count, Requests = lv2 });
                if (lv3.Any()) result.Add(new PendingApprovalGroup { ApproverLevel = 3, Count = lv3.Count, Requests = lv3 });

                return result;
            }

            // ================= DASHBOARD WIDGETS =================
            public async Task<List<WidgetCounterDto>> GetDashboardWidgetsAsync(
                string employeeCode,
                string deptCode,
                CancellationToken ct = default)
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
                new() { Title = "Đơn chờ duyệt",    Value = pendingCount.ToString(),                    Icon = "Icons.Material.Filled.HourglassEmpty", Color = "warning", Link = "/approve/list" },
                new() { Title = "Vắng mặt hôm nay", Value = absentToday.ToString(),                    Icon = "Icons.Material.Filled.People",          Color = "info",    Link = "/leave/department-status" },
                new() { Title = "Cảnh báo",          Value = absentToday > 10 ? "Cao" : "Bình thường", Icon = "Icons.Material.Filled.Warning",         Color = absentToday > 10 ? "error" : "success" }
            };
            }

            // ================= RECENT HISTORY =================
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
                        FromDate = x.StartDate,
                        ToDate = x.EndDate,
                        Status = x.RequestStatus ?? LeaveStatus.Pending,
                        ApproverName = x.Level1ApproveName ?? "N/A"
                    })
                    .ToListAsync(ct);
            }

            // ================= SIMPLE BALANCE =================
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

            // ================= LEAVE HISTORY =================
            public async Task<List<LeaveDaysViewModel>> GetHistoryAsync(
                string employeeCode,
                int? year,
                string? status,
                CancellationToken ct = default)
            {
                try
                {
                    var query = _db.VF03leaveDays
                        .AsNoTracking()
                        .Where(x => x.EmployeeCode == employeeCode && x.IsActive == true);

                    if (year.HasValue)
                        query = query.Where(x => x.StartDate.HasValue && x.StartDate.Value.Year == year.Value);

                    if (!string.IsNullOrEmpty(status))
                        query = query.Where(x => x.RequestStatus == status);

                    return await query
                        .OrderByDescending(x => x.RegisterDate)
                        .Select(x => new LeaveDaysViewModel
                        {
                            Id = x.Id ?? 0,
                            WorkYear = x.WorkYear ?? DateTime.Now.Year,
                            RegisterDate = x.RegisterDate ?? DateTime.Now,
                            StartDate = x.StartDate ?? DateTime.Now,
                            EndDate = x.EndDate ?? DateTime.Now,
                            TotalDay = x.TotalDay ?? 0,
                            LeaveTypeName = x.LeaveDay,
                            LeaveReason = x.LeaveReason,
                            RequestStatus = x.RequestStatus,
                            Level1ApproveName = x.Level1ApproveName,
                            Level1ApproveEmail = x.Level1ApproveEmail,
                            Level1IsApprove = x.Level1IsApprove ?? false,
                            Level2ApproveName = x.Level2ApproveName,
                            Level2ApproveEmail = x.Level2ApproveEmail,
                            Level2IsApprove = x.Level2IsApprove ?? false,
                            Level3ApproveName = x.Level3ApproveName,
                            Level3ApproveEmail = x.Level3ApproveEmail,
                            Level3IsApprove = x.Level3IsApprove ?? false,
                        })
                        .ToListAsync(ct);
                }
                catch
                {
                    return new List<LeaveDaysViewModel>();
                }
            }

            // ================= PRIVATE HELPERS =================

            // ✅ FIX CHÍNH: Bỏ hoàn toàn tham số cvCode + logic ép level sai
            private async Task<List<VF03leaveDaysApprover>> BuildApproverAsync(
                int level,
                string deptCode,
                CancellationToken ct)
            {
                if (level < 1 || level > 3) return new();
                return await GetApproverLevelAsync(level, deptCode, ct);
            }

            private async Task<List<VF03leaveDaysApprover>> GetApproverLevelAsync(
                int level,
                string deptCode,
                CancellationToken ct)
            {
                var baseQuery = _db.VF03leaveDaysApprovers
                    .AsNoTracking()
                    .Where(x => x.ApproveLevel == level && x.IsActive);

                // Ưu tiên approver đúng phòng ban
                var list = await baseQuery
                    .Where(x => x.DeptCode == deptCode)
                    .ToListAsync(ct);

                // Fallback: approver chung (ALL hoặc không có DeptCode)
                if (!list.Any())
                {
                    list = await baseQuery
                        .Where(x => string.IsNullOrEmpty(x.DeptCode) || x.DeptCode == "ALL")
                        .ToListAsync(ct);
                }

                return list;
            }

            private async Task<int> GetUserLevelAsync(string empCode, CancellationToken ct)
            {
                return await _db.F03leaveDaysApprovers
                    .Where(x => x.ApproveLevelCode == empCode)
                    .Select(x => x.ApproveLevel)
                    .FirstOrDefaultAsync(ct);
            }

            private async Task<string> ResolveApproverEmailAsync(string employeeCode, CancellationToken ct)
            {
                var email = await _db.F03leaveDaysApprovers
                    .Where(x => x.ApproveLevelCode == employeeCode && x.IsActive)
                    .Select(x => x.ApproveLevelEmail)
                    .FirstOrDefaultAsync(ct);

                if (!string.IsNullOrEmpty(email)) return email;

                // Fallback: email từ bảng nhân viên
                return await _db.VF03employees
                    .Where(x => x.EmployeeCode == employeeCode)
                    .Select(x => x.EmailAddress)
                    .FirstOrDefaultAsync(ct) ?? "";
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

            private async Task<List<string>> GetHolidayNotCountAsync(CancellationToken ct)
            {
                var dates = await _db.CompanyHolidays
                    .AsNoTracking()
                    .Where(x => x.TinhPhep == 0 && x.HolidayDate != null)
                    .Select(x => x.HolidayDate)
                    .ToListAsync(ct);

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
                    Id = d.DetailId.ToString(),
                    Inforregister = d.LeaveId.ToString(),
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

            private static string GetStatusClass(string? status) => status switch
            {
                "Approved" => "event-approved",
                "Rejected" => "event-rejected",
                "ApprovedLv1" => "event-lv1",
                "ApprovedLv2" => "event-lv2",
                _ => "event-pending"
            };

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
        }
    }

