// FVN_REGISTER.API/Services/Reports/LeaveReportService.cs
using FVN_REGISTER.Contract.Dtos.Reports;
using FVN_REGISTER.Contract.Interfaces.Reports;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Services.Reports
{
    public class LeaveReportService : BaseService<LeaveReportService>, IReportService
    {
        private readonly FVNWEBAPPContext _db;

        public LeaveReportService(
            FVNWEBAPPContext db,
            ILogger<LeaveReportService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _db = db;
        }

        public bool CanHandle(ReportType type) => type is
            ReportType.LeaveBalance or
            ReportType.LeaveSummaryByDept or
            ReportType.LeaveSummaryByEmployee or
            ReportType.LeaveDetail or
            ReportType.LeaveApprovalStatus;

        // ================= DISPATCH =================
        public async Task<ServiceResult<ReportResultDto>> GetReportAsync(
            ReportQueryDto query,
            CurrentUser user,
            CancellationToken ct = default)
        {
            try
            {
                return query.Type switch
                {
                    ReportType.LeaveBalance => await GetLeaveBalanceAsync(query, user, ct),
                    ReportType.LeaveSummaryByDept => await GetSummaryByDeptAsync(query, user, ct),
                    ReportType.LeaveSummaryByEmployee => await GetSummaryByEmployeeAsync(query, user, ct),
                    ReportType.LeaveDetail => await GetLeaveDetailAsync(query, user, ct),
                    ReportType.LeaveApprovalStatus => await GetApprovalStatusAsync(query, user, ct),
                    _ => ServiceResult<ReportResultDto>.Fail("Loại báo cáo không hợp lệ")
                };
            }
            catch (Exception ex)
            {
                return InternalError<ReportResultDto>(ex, "Lỗi tải báo cáo");
            }
        }

        // ================= EXPORT EXCEL =================
        public Task<ServiceResult<byte[]>> ExportExcelAsync(
            ReportQueryDto query,
            CurrentUser user,
            CancellationToken ct = default)
        {
            // Stub — tích hợp ClosedXML hoặc EPPlus sau
            return Task.FromResult(
                ServiceResult<byte[]>.Fail("Chức năng xuất Excel đang phát triển."));
        }

        // ================= LEAVE BALANCE =================
        private async Task<ServiceResult<ReportResultDto>> GetLeaveBalanceAsync(
            ReportQueryDto query, CurrentUser user, CancellationToken ct)
        {
            var year = query.WorkYear ?? DateTime.Now.Year;
            var isAdmin = user.IsAdmin() || user.IsSuperAdmin();

            var q = _db.VF03phepTons.AsNoTracking()
                .Where(x => x.WorkYear == year && x.IsActive);

            // --- SỬA THÀNH CODE ĐÚNG ---
            if (!isAdmin)
            {
                q = q.Where(x => x.EmployeeCode == user.EmployeeCode);
            }
            else
            {
                // Admin chọn phòng ban nào thì lọc phòng ban đó
                if (!string.IsNullOrEmpty(query.DeptCode))
                    q = q.Where(x => x.DeptCode == query.DeptCode);

                // Admin chọn nhân viên nào thì lọc chính xác nhân viên đó (Bổ sung thêm)
                if (!string.IsNullOrEmpty(query.EmployeeCode))
                    q = q.Where(x => x.EmployeeCode == query.EmployeeCode);
            }

            var data = await q.OrderBy(x => x.DeptCode)
                              .ThenBy(x => x.EmployeeName)
                              .ToListAsync(ct);

            var rows = data.Select(x => new Dictionary<string, object?>
            {
                ["EmployeeCode"] = x.EmployeeCode,
                ["EmployeeName"] = x.EmployeeName,
                ["DeptName"] = x.DeptName,
                ["TongPhep"] = x.TongPhep,
                ["SoNgayNghiPhep"] = x.SoNgayNghiPhep,
                ["PhepTon"] = x.PhepTon ?? 0m,
            }).ToList();

            var result = new ReportResultDto
            {
                Type = ReportType.LeaveBalance,
                Title = $"Báo cáo số dư phép năm {year}",
                TotalRows = rows.Count,
                Columns = new()
                {
                    new() { Field = "EmployeeCode",   Header = "Mã NV" },
                    new() { Field = "EmployeeName",   Header = "Họ tên" },
                    new() { Field = "DeptName",       Header = "Phòng ban" },
                    new() { Field = "TongPhep",       Header = "Tổng phép",  DataType = "decimal", Format = "N1" },
                    new() { Field = "SoNgayNghiPhep", Header = "Đã dùng",    DataType = "decimal", Format = "N1" },
                    new() { Field = "PhepTon",        Header = "Còn lại",    DataType = "decimal", Format = "N1" },
                },
                Rows = rows,
                Summary = new()
                {
                    ["TotalEmployees"] = data.Count,
                    ["TotalEntitled"] = data.Sum(x => x.TongPhep),
                    ["TotalUsed"] = data.Sum(x => x.SoNgayNghiPhep),
                    ["TotalRemaining"] = data.Sum(x => x.PhepTon ?? 0m),
                },
                Chart = BuildBalanceChart(data),
            };

            return ServiceResult<ReportResultDto>.Ok(result);
        }

        // ================= SUMMARY BY DEPT =================
        private async Task<ServiceResult<ReportResultDto>> GetSummaryByDeptAsync(
     ReportQueryDto query, CurrentUser user, CancellationToken ct)
        {
            var year = query.WorkYear ?? DateTime.Now.Year;
            var isAdmin = user.IsAdmin() || user.IsSuperAdmin();
            var isManager = user.IsApprover() || user.LevelApprove > 0; // Thêm check quyền quản lý

            var q = _db.VF03phepTons.AsNoTracking()
                .Where(x => x.WorkYear == year && x.IsActive);

            // PHÂN QUYỀN LỌC DỮ LIỆU
            if (isAdmin)
            {
                // Admin: Nếu chọn phòng ban trên UI thì lọc theo phòng ban đó
                if (!string.IsNullOrEmpty(query.DeptCode))
                    q = q.Where(x => x.DeptCode == query.DeptCode);
            }
            else if (isManager)
            {
                // Quản lý/Trưởng phòng: Chỉ được xem phòng ban của chính mình
                q = q.Where(x => x.DeptCode == user.DeptCode);
            }
            else
            {
                // Nhân viên thường: Không có quyền xem tab tổng hợp phòng ban này
                return ServiceResult<ReportResultDto>.Fail("Bạn không có quyền xem báo cáo phòng ban.");
            }

            var data = await q
                .GroupBy(x => new { x.DeptCode, x.DeptName })
                .Select(g => new
                {
                    g.Key.DeptCode,
                    g.Key.DeptName,
                    EmployeeCount = g.Count(),
                    TotalEntitled = g.Sum(x => x.TongPhep),
                    TotalUsed = g.Sum(x => x.SoNgayNghiPhep),
                    TotalRemaining = g.Sum(x => (decimal?)(x.PhepTon ?? 0)) ?? 0m,
                })
                .OrderBy(x => x.DeptName)
                .ToListAsync(ct);

            var rows = data.Select(x => new Dictionary<string, object?>
            {
                ["DeptCode"] = x.DeptCode,
                ["DeptName"] = x.DeptName,
                ["EmployeeCount"] = x.EmployeeCount,
                ["TotalEntitled"] = x.TotalEntitled,
                ["TotalUsed"] = x.TotalUsed,
                ["TotalRemaining"] = x.TotalRemaining,
            }).ToList();

            var result = new ReportResultDto
            {
                Type = ReportType.LeaveSummaryByDept,
                Title = $"Tổng hợp nghỉ phép theo phòng ban năm {year}",
                TotalRows = rows.Count,
                Columns = new()
        {
            new() { Field = "DeptCode",       Header = "Mã phòng" },
            new() { Field = "DeptName",       Header = "Phòng ban" },
            new() { Field = "EmployeeCount",  Header = "Số NV",       DataType = "number" },
            new() { Field = "TotalEntitled",  Header = "Tổng phép",   DataType = "decimal", Format = "N1" },
            new() { Field = "TotalUsed",      Header = "Đã dùng",     DataType = "decimal", Format = "N1" },
            new() { Field = "TotalRemaining", Header = "Còn lại",     DataType = "decimal", Format = "N1" },
        },
                Rows = rows,
                Summary = new()
                {
                    ["TotalDepts"] = data.Count,
                    ["TotalEntitled"] = data.Sum(x => x.TotalEntitled),
                    ["TotalUsed"] = data.Sum(x => x.TotalUsed),
                    ["TotalRemaining"] = data.Sum(x => x.TotalRemaining),
                },
                Chart = new()
                {
                    ChartType = "bar",
                    Labels = data.Select(x => x.DeptName ?? x.DeptCode ?? "").ToList(),
                    Series = new()
            {
                new() { Name = "Số ngày đã nghỉ", Data = data.Select(x => x.TotalUsed).ToList() }
            }
                }
            };

            return ServiceResult<ReportResultDto>.Ok(result);
        }

        // ================= SUMMARY BY EMPLOYEE =================
        private async Task<ServiceResult<ReportResultDto>> GetSummaryByEmployeeAsync(
    ReportQueryDto query, CurrentUser user, CancellationToken ct)
        {
            var isAdmin = user.IsAdmin() || user.IsSuperAdmin();
            var isManager = user.IsApprover() || user.LevelApprove > 0; // Thêm check quyền quản lý

            var q = _db.VF03leaveDays.AsNoTracking()
                .Where(x => x.IsActive == true
                         && x.StartDate >= query.FromDate
                         && x.StartDate <= query.ToDate);

            // PHÂN QUYỀN 3 CẤP ĐỘ
            if (isAdmin)
            {
                // 1. ADMIN: Xem toàn công ty, lọc tự do
                if (!string.IsNullOrEmpty(query.DeptCode))
                    q = q.Where(x => x.DeptCode == query.DeptCode);

                if (!string.IsNullOrEmpty(query.EmployeeCode))
                    q = q.Where(x => x.EmployeeCode == query.EmployeeCode);
            }
            else if (isManager)
            {
                // 2. QUẢN LÝ / TRƯỞNG PHÒNG: Khóa cứng phòng ban của mình
                q = q.Where(x => x.DeptCode == user.DeptCode);

                // Nhưng được chọn lọc đích danh nhân viên cấp dưới trong phòng
                if (!string.IsNullOrEmpty(query.EmployeeCode))
                    q = q.Where(x => x.EmployeeCode == query.EmployeeCode);
            }
            else
            {
                // 3. NHÂN VIÊN THƯỜNG: Chỉ được xem chính mình
                q = q.Where(x => x.EmployeeCode == user.EmployeeCode);
            }

            var data = await q
                .GroupBy(x => new { x.EmployeeCode, x.EmployeeName, x.DeptCode, x.DeptName })
                .Select(g => new
                {
                    g.Key.EmployeeCode,
                    g.Key.EmployeeName,
                    g.Key.DeptCode,
                    g.Key.DeptName,
                    TotalRequests = g.Count(),
                    TotalDays = g.Sum(x => x.TotalDay ?? 0),
                    ApprovedDays = g.Where(x => x.RequestStatus == LeaveStatus.Approved)
                                    .Sum(x => x.TotalDay ?? 0),
                    PendingCount = g.Count(x => LeaveStatus.ActiveStatuses.Contains(x.RequestStatus!)),
                })
                .OrderBy(x => x.DeptName)
                .ThenBy(x => x.EmployeeName)
                .ToListAsync(ct);

            var rows = data.Select(x => new Dictionary<string, object?>
            {
                ["EmployeeCode"] = x.EmployeeCode,
                ["EmployeeName"] = x.EmployeeName,
                ["DeptName"] = x.DeptName,
                ["TotalRequests"] = x.TotalRequests,
                ["TotalDays"] = x.TotalDays,
                ["ApprovedDays"] = x.ApprovedDays,
                ["PendingCount"] = x.PendingCount,
            }).ToList();

            var result = new ReportResultDto
            {
                Type = ReportType.LeaveSummaryByEmployee,
                Title = $"Tổng hợp nghỉ phép theo nhân viên ({query.FromDate:dd/MM/yyyy} - {query.ToDate:dd/MM/yyyy})",
                TotalRows = rows.Count,
                Columns = new()
        {
            new() { Field = "EmployeeCode",  Header = "Mã NV" },
            new() { Field = "EmployeeName",  Header = "Họ tên" },
            new() { Field = "DeptName",      Header = "Phòng ban" },
            new() { Field = "TotalRequests", Header = "Số đơn",           DataType = "number" },
            new() { Field = "TotalDays",     Header = "Tổng ngày",       DataType = "decimal", Format = "N1" },
            new() { Field = "ApprovedDays",  Header = "Đã duyệt (ngày)", DataType = "decimal", Format = "N1" },
            new() { Field = "PendingCount",  Header = "Đang chờ",        DataType = "number" },
        },
                Rows = rows,
                Summary = new()
                {
                    ["TotalEmployees"] = data.Count,
                    ["TotalRequests"] = data.Sum(x => x.TotalRequests),
                    ["TotalDays"] = data.Sum(x => x.TotalDays),
                    ["ApprovedDays"] = data.Sum(x => x.ApprovedDays),
                },
            };

            return ServiceResult<ReportResultDto>.Ok(result);
        }

        // ================= LEAVE DETAIL =================
        private async Task<ServiceResult<ReportResultDto>> GetLeaveDetailAsync(
            ReportQueryDto query, CurrentUser user, CancellationToken ct)
        {
            var isAdmin = user.IsAdmin() || user.IsSuperAdmin();

            var q = _db.VF03leaveDays.AsNoTracking()
                .Where(x => x.IsActive == true
                         && x.StartDate >= query.FromDate
                         && x.StartDate <= query.ToDate);

            if (!isAdmin)
                q = q.Where(x => x.EmployeeCode == user.EmployeeCode);
            else if (!string.IsNullOrEmpty(query.DeptCode))
                q = q.Where(x => x.DeptCode == query.DeptCode);
            if (!string.IsNullOrEmpty(query.EmployeeCode))
                q = q.Where(x => x.EmployeeCode == query.EmployeeCode);

            var totalCount = await q.CountAsync(ct);

            var data = await q
                .OrderByDescending(x => x.RegisterDate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(ct);

            var rows = data.Select(x => new Dictionary<string, object?>
            {
                ["Id"] = x.Id,
                ["EmployeeCode"] = x.EmployeeCode,
                ["EmployeeName"] = x.EmployeeName,
                ["DeptName"] = x.DeptName,
                ["RegisterDate"] = x.RegisterDate,
                ["StartDate"] = x.StartDate,
                ["EndDate"] = x.EndDate,
                ["TotalDay"] = x.TotalDay,
                ["RequestStatus"] = LeaveStatus.GetDisplayName(x.RequestStatus ?? ""),
                ["Level1Approver"] = x.Level1ApproveName,
                ["Level2Approver"] = x.Level2ApproveName,
            }).ToList();

            var result = new ReportResultDto
            {
                Type = ReportType.LeaveDetail,
                Title = $"Chi tiết đơn nghỉ ({query.FromDate:dd/MM/yyyy} - {query.ToDate:dd/MM/yyyy})",
                TotalRows = totalCount,
                Columns = new()
                {
                    new() { Field = "EmployeeCode",  Header = "Mã NV" },
                    new() { Field = "EmployeeName",  Header = "Họ tên" },
                    new() { Field = "DeptName",      Header = "Phòng ban" },
                    new() { Field = "RegisterDate",  Header = "Ngày viết đơn", DataType = "date", Format = "dd/MM/yyyy" },
                    new() { Field = "StartDate",     Header = "Từ ngày",        DataType = "date", Format = "dd/MM/yyyy" },
                    new() { Field = "EndDate",       Header = "Đến ngày",       DataType = "date", Format = "dd/MM/yyyy" },
                    new() { Field = "TotalDay",      Header = "Số ngày",        DataType = "decimal", Format = "N1" },
                    new() { Field = "RequestStatus", Header = "Trạng thái" },
                    new() { Field = "Level1Approver",Header = "Cấp 1" },
                    new() { Field = "Level2Approver",Header = "Cấp 2" },
                },
                Rows = rows,
                Summary = new()
                {
                    ["TotalRequests"] = totalCount,
                    ["TotalDays"] = data.Sum(x => x.TotalDay ?? 0),
                },
            };

            return ServiceResult<ReportResultDto>.Ok(result);
        }

        // ================= APPROVAL STATUS =================
        private async Task<ServiceResult<ReportResultDto>> GetApprovalStatusAsync(
            ReportQueryDto query, CurrentUser user, CancellationToken ct)
        {
            var q = _db.VF03leaveDays.AsNoTracking()
                .Where(x => x.IsActive == true
                         && x.RegisterDate >= query.FromDate
                         && x.RegisterDate <= query.ToDate);

            if (!string.IsNullOrEmpty(query.DeptCode))
                q = q.Where(x => x.DeptCode == query.DeptCode);

            var data = await q
                .GroupBy(x => x.RequestStatus)
                .Select(g => new
                {
                    Status = g.Key ?? "Unknown",
                    Count = g.Count(),
                    Days = g.Sum(x => x.TotalDay ?? 0),
                })
                .ToListAsync(ct);

            var rows = data.Select(x => new Dictionary<string, object?>
            {
                ["Status"] = LeaveStatus.GetDisplayName(x.Status),
                ["StatusRaw"] = x.Status,
                ["Count"] = x.Count,
                ["TotalDays"] = x.Days,
            }).ToList();

            var result = new ReportResultDto
            {
                Type = ReportType.LeaveApprovalStatus,
                Title = "Báo cáo trạng thái phê duyệt",
                TotalRows = rows.Count,
                Columns = new()
                {
                    new() { Field = "Status",    Header = "Trạng thái" },
                    new() { Field = "Count",     Header = "Số đơn",   DataType = "number" },
                    new() { Field = "TotalDays", Header = "Tổng ngày", DataType = "decimal", Format = "N1" },
                },
                Rows = rows,
                Summary = new()
                {
                    ["TotalRequests"] = data.Sum(x => x.Count),
                    ["TotalDays"] = data.Sum(x => x.Days),
                },
                Chart = new()
                {
                    ChartType = "pie",
                    Labels = data.Select(x => LeaveStatus.GetDisplayName(x.Status)).ToList(),
                    Series = new()
                    {
                        new() { Name = "Số đơn", Data = data.Select(x => (decimal)x.Count).ToList() }
                    }
                },
            };

            return ServiceResult<ReportResultDto>.Ok(result);
        }

        // ================= HELPER =================
        // 1. Lấy danh sách phòng ban cho Dropdown
        public async Task<ServiceResult<List<KeyValuePair<string, string>>>> GetLookupDepartmentsAsync(
     CurrentUser user, CancellationToken ct = default)
        {
            var isAdmin = user.IsAdmin() || user.IsSuperAdmin();

            var q = _db.VF03phepTons.AsNoTracking()
                .Where(x => x.IsActive && !string.IsNullOrEmpty(x.DeptCode));

            // Nếu không phải Admin (kể cả là Quản lý hay Nhân viên), chỉ hiển thị phòng ban của chính họ
            if (!isAdmin)
            {
                q = q.Where(x => x.DeptCode == user.DeptCode);
            }

            var depts = await q
                .Select(x => new { x.DeptCode, x.DeptName })
                .Distinct()
                .OrderBy(x => x.DeptName)
                .ToListAsync(ct);

            var result = depts.Select(x => new KeyValuePair<string, string>(x.DeptCode!, x.DeptName ?? x.DeptCode!)).ToList();
            return ServiceResult<List<KeyValuePair<string, string>>>.Ok(result);
        }

        // 2. Tìm kiếm nhân viên phục vụ Autocomplete (Tìm theo Mã hoặc Tên)
        public async Task<ServiceResult<List<KeyValuePair<string, string>>>> SearchLookupEmployeesAsync(
     string filterText, CurrentUser user, string? deptCode = null, CancellationToken ct = default)
        {
            var isAdmin = user.IsAdmin() || user.IsSuperAdmin();
            var isManager = user.IsApprover() || user.LevelApprove > 0;

            var query = _db.VF03phepTons.AsNoTracking().Where(x => x.IsActive);

            if (isAdmin)
            {
                // Admin: Nếu đang chọn 1 phòng ban cụ thể trên UI, chỉ gợi ý nhân viên phòng đó
                if (!string.IsNullOrEmpty(deptCode))
                    query = query.Where(x => x.DeptCode == deptCode);
            }
            else if (isManager)
            {
                // Quản lý: Chỉ được tìm kiếm những nhân viên thuộc phòng ban của mình
                query = query.Where(x => x.DeptCode == user.DeptCode);
            }
            else
            {
                // Nhân viên thường: Chỉ tìm thấy chính mình, không hiển thị người khác
                query = query.Where(x => x.EmployeeCode == user.EmployeeCode);
            }

            // Áp dụng từ khóa gõ tìm kiếm (Mã hoặc Tên)
            if (!string.IsNullOrEmpty(filterText))
            {
                query = query.Where(x => x.EmployeeCode.Contains(filterText) || x.EmployeeName!.Contains(filterText));
            }

            var emps = await query
                .Select(x => new { x.EmployeeCode, x.EmployeeName })
                .Distinct()
                .Take(20)
                .OrderBy(x => x.EmployeeCode)
                .ToListAsync(ct);

            var result = emps.Select(x => new KeyValuePair<string, string>(x.EmployeeCode, $"{x.EmployeeCode} - {x.EmployeeName}")).ToList();
            return ServiceResult<List<KeyValuePair<string, string>>>.Ok(result);
        }
        private static ReportChartData? BuildBalanceChart(List<VF03phepTon> data)
        {
            if (!data.Any()) return null;
            var top = data.OrderByDescending(x => x.SoNgayNghiPhep).Take(10).ToList();
            return new()
            {
                ChartType = "bar",
                Labels = top.Select(x => x.EmployeeName ?? x.EmployeeCode).ToList(),
                Series = new()
                {
                    new() { Name = "Đã dùng", Data = top.Select(x => x.SoNgayNghiPhep).ToList() },
                    new() { Name = "Còn lại",  Data = top.Select(x => x.PhepTon ?? 0m).ToList() },
                }
            };
        }
    }
}