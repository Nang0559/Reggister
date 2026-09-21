using FVN_REGISTER.Infrastructure.Services.Reports;
using FVN_REGISTER.Application.Interfaces.Reports;

using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Reports;

using FVN_REGISTER.Core.Constants;

using FVN_REGISTER.Core.Extensions;
using FVN_REGISTER.Core.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace FVN_REGISTER.Infrastructure.Services.Reports
{
    public class LeaveReportService
        : BaseReportService<LeaveReportService>, IReportService
    {
        public LeaveReportService(
            IUnitOfWork uow,
            ILogger<LeaveReportService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(uow, logger, options)
        { }

        public override bool CanHandle(ReportType type) => type is
            ReportType.LeaveBalance or
            ReportType.LeaveSummaryByDept or
            ReportType.LeaveSummaryByEmployee or
            ReportType.LeaveDetail or
            ReportType.LeaveApprovalStatus;

        public override async Task<ServiceResult<ReportResultDto>> GetReportAsync(
            ReportQueryDto query, UserIdentityDto user, CancellationToken ct = default)
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

        // ════════════════════════════════════════════════════════════════
        // LEAVE BALANCE — nguồn: VF03LeaveBalance (theo WorkYear, quota tính sẵn ở SQL)
        // ════════════════════════════════════════════════════════════════
        private async Task<ServiceResult<ReportResultDto>> GetLeaveBalanceAsync(
            ReportQueryDto query, UserIdentityDto user, CancellationToken ct)
        {
            var year = query.WorkYear ?? DateTime.Now.Year;


            var q = _uow.Repository<VF03LeaveBalance>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive && x.WorkYear == year);

            if (!string.IsNullOrEmpty(query.DeptCode))
                q = q.Where(x => x.DeptCode == query.DeptCode);
            if (!string.IsNullOrEmpty(query.EmployeeCode))
                q = q.Where(x => x.EmployeeCode == query.EmployeeCode);

            var data = await q
                .OrderBy(x => x.DeptName).ThenBy(x => x.EmployeeName)
                .ToListAsync(ct);

            var rows = data.Select(x => new Dictionary<string, object?>
            {
                ["EmployeeCode"] = x.EmployeeCode,
                ["EmployeeName"] = x.EmployeeName ?? x.EmployeeCode,
                ["DeptName"] = x.DeptName ?? x.DeptCode ?? "",
                ["TotalEntitledLeave"] = x.TotalEntitledLeave,
                ["LeaveDaysUsed"] = x.LeaveDaysUsed,
                ["TotalDaysOff"] = x.TotalDaysOff,
                ["RemainingLeave"] = x.RemainingLeave ?? 0,
            }).ToList();

            return ServiceResult<ReportResultDto>.Ok(new ReportResultDto
            {
                Type = ReportType.LeaveBalance,
                Title = $"Số dư phép năm {year}",
                TotalRows = rows.Count,
                Columns = new()
            {
                new() { Field = "EmployeeCode",       Header = "Mã NV" },
                new() { Field = "EmployeeName",       Header = "Họ tên" },
                new() { Field = "DeptName",           Header = "Phòng ban" },
                new() { Field = "TotalEntitledLeave", Header = "Phép được cấp", DataType = "decimal", Format = "N1" },
                new() { Field = "LeaveDaysUsed",       Header = "Đã dùng (phép)", DataType = "decimal", Format = "N1" },
                new() { Field = "TotalDaysOff",       Header = "Tổng ngày nghỉ", DataType = "decimal", Format = "N1" },
                new() { Field = "RemainingLeave",     Header = "Còn lại",       DataType = "decimal", Format = "N1" },
            },
                Rows = rows,
                Summary = new()
                {
                    ["TotalEmployees"] = data.Count,
                    ["TotalEntitled"] = data.Sum(x => x.TotalEntitledLeave),
                    ["TotalUsed"] = data.Sum(x => x.LeaveDaysUsed),
                    ["TotalRemaining"] = data.Sum(x => x.RemainingLeave ?? 0),
                },
            });
        }

        // ════════════════════════════════════════════════════════════════
        // SUMMARY BY DEPT — nguồn: VF03LeaveRequestDetail (group theo ngày nghỉ thực)
        // ════════════════════════════════════════════════════════════════
        private async Task<ServiceResult<ReportResultDto>> GetSummaryByDeptAsync(
            ReportQueryDto query, UserIdentityDto user, CancellationToken ct)
        {
            var fromDate = query.FromDate ?? DateTime.Today.AddMonths(-1);
            var toDate = query.ToDate ?? DateTime.Today;


            var q = _uow.Repository<VF03LeaveRequestDetail>().Query()
                .AsNoTracking()
                .Where(x => x.LeaveDate >= DateOnly.FromDateTime(fromDate)
                         && x.LeaveDate <= DateOnly.FromDateTime(toDate)
                         && x.IsCountedAsLeave);

            if (string.IsNullOrWhiteSpace(query.DeptCode) && string.IsNullOrWhiteSpace(query.EmployeeCode))
                return ServiceResult<ReportResultDto>.Fail("Thiếu phạm vi dữ liệu báo cáo nghỉ phép.");
            if (!string.IsNullOrEmpty(query.DeptCode))
                q = q.Where(x => x.DeptCode == query.DeptCode);
            if (!string.IsNullOrEmpty(query.EmployeeCode))
                q = q.Where(x => x.EmployeeCode == query.EmployeeCode);

            var data = await q
                .GroupBy(x => new { x.DeptCode, x.DeptName })
                .Select(g => new
                {
                    g.Key.DeptCode,
                    g.Key.DeptName,
                    EmployeeCount = g.Select(x => x.EmployeeCode).Distinct().Count(),
                    TotalUsed = g.Sum(x => x.DayValue),
                })
                .OrderBy(x => x.DeptName)
                .ToListAsync(ct);

            var rows = data.Select(x => new Dictionary<string, object?>
            {
                ["DeptCode"] = x.DeptCode,
                ["DeptName"] = x.DeptName ?? x.DeptCode ?? "",
                ["EmployeeCount"] = x.EmployeeCount,
                ["TotalUsed"] = x.TotalUsed,
            }).ToList();

            return ServiceResult<ReportResultDto>.Ok(new ReportResultDto
            {
                Type = ReportType.LeaveSummaryByDept,
                Title = $"Tổng hợp nghỉ phép theo phòng ban ({fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy})",
                TotalRows = rows.Count,
                Columns = new()
                {
                    new() { Field = "DeptCode",      Header = "Mã phòng" },
                    new() { Field = "DeptName",      Header = "Phòng ban" },
                    new() { Field = "EmployeeCount", Header = "Số NV",   DataType = "number" },
                    new() { Field = "TotalUsed",     Header = "Đã dùng (ngày)", DataType = "decimal", Format = "N1" },
                },
                Rows = rows,
                Summary = new()
                {
                    ["TotalDepts"] = data.Count,
                    ["TotalUsed"] = data.Sum(x => x.TotalUsed),
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
            });
        }

        // ════════════════════════════════════════════════════════════════
        // SUMMARY BY EMPLOYEE — nguồn: VF03LeaveRequest (theo đơn, có TotalDay sẵn)
        // ════════════════════════════════════════════════════════════════
        private async Task<ServiceResult<ReportResultDto>> GetSummaryByEmployeeAsync(
            ReportQueryDto query, UserIdentityDto user, CancellationToken ct)
        {
            var fromDate = query.FromDate ?? DateTime.Today.AddMonths(-1);
            var toDate = query.ToDate ?? DateTime.Today;


            var q = _uow.Repository<VF03LeaveRequest>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true
                         && x.StartDate >= fromDate
                         && x.StartDate <= toDate);

            if (isAdmin)
            {
                if (!string.IsNullOrEmpty(query.DeptCode))
                    q = q.Where(x => x.DeptCode == query.DeptCode);
                if (!string.IsNullOrEmpty(query.EmployeeCode))
                    q = q.Where(x => x.EmployeeCode == query.EmployeeCode);
            }
            else if (isManager)
            {
                q = q.Where(x => x.DeptCode == user.DeptCode);
                if (!string.IsNullOrEmpty(query.EmployeeCode))
                    q = q.Where(x => x.EmployeeCode == query.EmployeeCode);
            }
            else
            {
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
                    TotalDays = g.Sum(x => x.TotalDay),
                    ApprovedDays = g.Where(x => x.RequestStatus == ApprovalStatus.Approved)
                                     .Sum(x => x.TotalDay),
                    PendingCount = g.Count(x =>
                        x.RequestStatus != ApprovalStatus.Approved &&
                        x.RequestStatus != ApprovalStatus.Rejected &&
                        x.RequestStatus != ApprovalStatus.Cancelled),
                })
                .OrderBy(x => x.DeptName).ThenBy(x => x.EmployeeName)
                .ToListAsync(ct);

            var rows = data.Select(x => new Dictionary<string, object?>
            {
                ["EmployeeCode"] = x.EmployeeCode,
                ["EmployeeName"] = x.EmployeeName,
                ["DeptName"] = x.DeptName ?? x.DeptCode ?? "",
                ["TotalRequests"] = x.TotalRequests,
                ["TotalDays"] = x.TotalDays,
                ["ApprovedDays"] = x.ApprovedDays,
                ["PendingCount"] = x.PendingCount,
            }).ToList();

            return ServiceResult<ReportResultDto>.Ok(new ReportResultDto
            {
                Type = ReportType.LeaveSummaryByEmployee,
                Title = $"Tổng hợp nghỉ phép theo nhân viên ({fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy})",
                TotalRows = rows.Count,
                Columns = new()
                {
                    new() { Field = "EmployeeCode",  Header = "Mã NV" },
                    new() { Field = "EmployeeName",  Header = "Họ tên" },
                    new() { Field = "DeptName",      Header = "Phòng ban" },
                    new() { Field = "TotalRequests", Header = "Số đơn",          DataType = "number" },
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
            });
        }

        // ════════════════════════════════════════════════════════════════
        // LEAVE DETAIL — nguồn: VF03LeaveRequest (đã có EmployeeName/DeptName sẵn)
        // ════════════════════════════════════════════════════════════════
        private async Task<ServiceResult<ReportResultDto>> GetLeaveDetailAsync(
            ReportQueryDto query, UserIdentityDto user, CancellationToken ct)
        {
            var fromDate = query.FromDate ?? DateTime.Today.AddMonths(-1);
            var toDate = query.ToDate ?? DateTime.Today;


            var q = _uow.Repository<VF03LeaveRequest>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true
                         && x.StartDate >= fromDate
                         && x.StartDate <= toDate);

            if (!string.IsNullOrEmpty(query.DeptCode))
                q = q.Where(x => x.DeptCode == query.DeptCode);
            if (!string.IsNullOrEmpty(query.EmployeeCode))
                q = q.Where(x => x.EmployeeCode == query.EmployeeCode);

            var totalCount = await q.CountAsync(ct);

            var data = await q
                .OrderByDescending(x => x.RegisterDate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(ct);

            var requestIds = data.Select(x => x.Id).ToList();
            var stepsMap = await LoadApprovalStepsMapAsync(RequestModule.Leave, requestIds, ct);

            var rows = data.Select(x => new Dictionary<string, object?>
            {
                ["Id"] = x.Id,
                ["EmployeeCode"] = x.EmployeeCode,
                ["EmployeeName"] = x.EmployeeName,
                ["DeptName"] = x.DeptName ?? x.DeptCode ?? "",
                ["RegisterDate"] = x.RegisterDate,
                ["StartDate"] = x.StartDate,
                ["EndDate"] = x.EndDate,
                ["TotalDay"] = x.TotalDay,
                ["RequestStatus"] = x.RequestStatus.ToDisplayName(),
                ["Level1Approver"] = GetApproverName(stepsMap, x.Id, level: 1),
                ["Level2Approver"] = GetApproverName(stepsMap, x.Id, level: 2),
                ["Level3Approver"] = GetApproverName(stepsMap, x.Id, level: 3),
            }).ToList();

            return ServiceResult<ReportResultDto>.Ok(new ReportResultDto
            {
                Type = ReportType.LeaveDetail,
                Title = $"Chi tiết đơn nghỉ phép ({fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy})",
                TotalRows = totalCount,
                Columns = new()
                {
                    new() { Field = "EmployeeCode",   Header = "Mã NV" },
                    new() { Field = "EmployeeName",   Header = "Họ tên" },
                    new() { Field = "DeptName",       Header = "Phòng ban" },
                    new() { Field = "RegisterDate",   Header = "Ngày đăng ký", DataType = "date" },
                    new() { Field = "StartDate",      Header = "Từ ngày",      DataType = "date" },
                    new() { Field = "EndDate",        Header = "Đến ngày",     DataType = "date" },
                    new() { Field = "TotalDay",       Header = "Số ngày",      DataType = "decimal", Format = "N1" },
                    new() { Field = "RequestStatus",  Header = "Trạng thái" },
                    new() { Field = "Level1Approver", Header = "Duyệt cấp 1" },
                    new() { Field = "Level2Approver", Header = "Duyệt cấp 2" },
                    new() { Field = "Level3Approver", Header = "Duyệt cấp 3" },
                },
                Rows = rows,
                Summary = new()
                {
                    ["TotalRequests"] = totalCount,
                    ["TotalDays"] = data.Sum(x => x.TotalDay),
                },
            });
        }

        // ════════════════════════════════════════════════════════════════
        // APPROVAL STATUS — nguồn: VF03LeaveRequest (enum sạch)
        // ════════════════════════════════════════════════════════════════
        private async Task<ServiceResult<ReportResultDto>> GetApprovalStatusAsync(
            ReportQueryDto query, UserIdentityDto user, CancellationToken ct)
        {
            var fromDate = query.FromDate ?? DateTime.Today.AddMonths(-1);
            var toDate = query.ToDate ?? DateTime.Today;

            var q = _uow.Repository<VF03LeaveRequest>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true
                         && x.RegisterDate >= fromDate
                         && x.RegisterDate <= toDate);

            if (!string.IsNullOrEmpty(query.DeptCode))
                q = q.Where(x => x.DeptCode == query.DeptCode);

            var data = await q
                .GroupBy(x => x.RequestStatus)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                    Days = g.Sum(x => x.TotalDay)
                })
                .ToListAsync(ct);

            var rows = data.Select(x => new Dictionary<string, object?>
            {
                ["Status"] = x.Status.ToDisplayName(),
                ["StatusRaw"] = x.Status.ToString(),
                ["Count"] = x.Count,
                ["TotalDays"] = x.Days,
            }).ToList();

            return ServiceResult<ReportResultDto>.Ok(new ReportResultDto
            {
                Type = ReportType.LeaveApprovalStatus,
                Title = "Báo cáo trạng thái phê duyệt",
                TotalRows = rows.Count,
                Columns = new()
                {
                    new() { Field = "Status",    Header = "Trạng thái" },
                    new() { Field = "Count",     Header = "Số đơn",    DataType = "number" },
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
                    Labels = data.Select(x => x.Status.ToDisplayName()).ToList(),
                    Series = new()
                    {
                        new() { Name = "Số đơn", Data = data.Select(x => (decimal)x.Count).ToList() }
                    }
                },
            });
        }
    }
}