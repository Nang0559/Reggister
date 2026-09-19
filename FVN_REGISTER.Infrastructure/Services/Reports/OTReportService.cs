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
    public class OTReportService
        : BaseReportService<OTReportService>, IReportService
    {
        public OTReportService(
            IUnitOfWork uow,
            ILogger<OTReportService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(uow, logger, options)
        { }

        public override bool CanHandle(ReportType type) => type is
            ReportType.OTSummaryByDept or
            ReportType.OTSummaryByEmployee or
            ReportType.OTDetail or
            ReportType.OTApprovalStatus or
            ReportType.OTLimitUsage;

        public override async Task<ServiceResult<ReportResultDto>> GetReportAsync(
            ReportQueryDto query, UserIdentityDto user, CancellationToken ct = default)
        {
            try
            {
                return query.Type switch
                {
                    ReportType.OTSummaryByDept => await GetOTSummaryByDeptAsync(query, user, ct),
                    ReportType.OTSummaryByEmployee => await GetOTSummaryByEmployeeAsync(query, user, ct),
                    ReportType.OTDetail => await GetOTDetailAsync(query, user, ct),
                    ReportType.OTApprovalStatus => await GetOTApprovalStatusAsync(query, user, ct),
                    ReportType.OTLimitUsage => await GetOTLimitUsageAsync(query, user, ct),
                    _ => ServiceResult<ReportResultDto>.Fail("Loại báo cáo OT không hợp lệ")
                };
            }
            catch (Exception ex)
            {
                return InternalError<ReportResultDto>(ex, "Lỗi tải báo cáo OT");
            }
        }

        private static readonly ApprovalStatus[] ActiveOrApprovedStatuses =
        {
            ApprovalStatus.Pending,
            ApprovalStatus.InProgress,
            ApprovalStatus.Approved
        };

        // ── OT Summary By Dept — nguồn: VF03OTRequestDetail (cấp nhân viên/theo ngày) ──
        private async Task<ServiceResult<ReportResultDto>> GetOTSummaryByDeptAsync(
            ReportQueryDto query, UserIdentityDto user, CancellationToken ct)
        {
            bool isAdmin = user.Permission.IsAdmin();
            bool isManager = user.Permission.IsApprover() || user.LevelApprove > 0;
            if (!isAdmin && !isManager) return ServiceResult<ReportResultDto>.Fail("Bạn không có quyền xem báo cáo OT theo phòng ban.");

            var fromDate = query.FromDate ?? DateTime.Today.AddMonths(-1);
            var toDate = query.ToDate ?? DateTime.Today;

            var q = _uow.Repository<VF03OTRequestDetail>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true
                         && x.OTDate >= DateOnly.FromDateTime(fromDate)
                         && x.OTDate <= DateOnly.FromDateTime(toDate)
                         && ActiveOrApprovedStatuses.Contains(x.RequestStatus));

            if (!isAdmin)
                q = q.Where(x => x.DeptCode == user.DeptCode);
            else if (!string.IsNullOrEmpty(query.DeptCode))
                q = q.Where(x => x.DeptCode == query.DeptCode);

            var data = await q
                .GroupBy(x => new { x.DeptCode, x.DeptName })
                .Select(g => new
                {
                    g.Key.DeptCode,
                    g.Key.DeptName,
                    EmployeeCount = g.Select(x => x.EmployeeCode).Distinct().Count(),
                    TotalOTHours = g.Sum(x => x.ActualHours ?? x.OTHours),
                    RequestCount = g.Select(x => x.OTRequestId).Distinct().Count(),
                })
                .OrderBy(x => x.DeptCode)
                .ToListAsync(ct);

            var rows = data.Select(x => new Dictionary<string, object?>
            {
                ["DeptCode"] = x.DeptCode,
                ["DeptName"] = x.DeptName ?? x.DeptCode ?? "",
                ["EmployeeCount"] = x.EmployeeCount,
                ["RequestCount"] = x.RequestCount,
                ["TotalOTHours"] = x.TotalOTHours,
            }).ToList();

            return ServiceResult<ReportResultDto>.Ok(new ReportResultDto
            {
                Type = ReportType.OTSummaryByDept,
                Title = $"Tổng hợp OT theo phòng ban ({query.FromDate:dd/MM/yyyy} - {query.ToDate:dd/MM/yyyy})",
                TotalRows = rows.Count,
                Columns = new()
                {
                    new() { Field = "DeptCode",      Header = "Mã phòng" },
                    new() { Field = "DeptName",      Header = "Phòng ban" },
                    new() { Field = "EmployeeCount", Header = "Số NV OT",    DataType = "number" },
                    new() { Field = "RequestCount",  Header = "Số phiếu",    DataType = "number" },
                    new() { Field = "TotalOTHours",  Header = "Tổng giờ OT", DataType = "decimal", Format = "N1" },
                },
                Rows = rows,
                Summary = new()
                {
                    ["TotalDepts"] = data.Count,
                    ["TotalRequests"] = data.Sum(x => x.RequestCount),
                    ["TotalOTHours"] = data.Sum(x => x.TotalOTHours),
                },
                Chart = new()
                {
                    ChartType = "bar",
                    Labels = rows.Select(r => r["DeptName"]?.ToString() ?? "").ToList(),
                    Series = new()
                    {
                        new() { Name = "Giờ OT", Data = data.Select(x => x.TotalOTHours).ToList() }
                    }
                }
            });
        }

        // ── OT Summary By Employee — nguồn: VF03OTRequestDetail ──
        private async Task<ServiceResult<ReportResultDto>> GetOTSummaryByEmployeeAsync(
            ReportQueryDto query, UserIdentityDto user, CancellationToken ct)
        {
            bool isAdmin = user.Permission.IsAdmin();
            bool isManager = user.Permission.IsApprover() || user.LevelApprove > 0;

            var fromDate = query.FromDate ?? DateTime.Today.AddMonths(-1);
            var toDate = query.ToDate ?? DateTime.Today;

            var q = _uow.Repository<VF03OTRequestDetail>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true
                         && x.OTDate >= DateOnly.FromDateTime(fromDate)
                         && x.OTDate <= DateOnly.FromDateTime(toDate));

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
                    RequestCount = g.Select(x => x.OTRequestId).Distinct().Count(),
                    TotalPlanned = g.Sum(x => x.OTHours),
                    TotalActual = g.Sum(x => x.ActualHours ?? 0),
                    ApprovedHours = g
                        .Where(x => x.RequestStatus == ApprovalStatus.Approved)
                        .Sum(x => x.ActualHours ?? x.OTHours),
                })
                .OrderBy(x => x.DeptCode).ThenBy(x => x.EmployeeCode)
                .ToListAsync(ct);

            var rows = data.Select(x => new Dictionary<string, object?>
            {
                ["EmployeeCode"] = x.EmployeeCode,
                ["EmployeeName"] = x.EmployeeName ?? x.EmployeeCode,
                ["DeptName"] = x.DeptName ?? x.DeptCode ?? "",
                ["RequestCount"] = x.RequestCount,
                ["TotalPlanned"] = x.TotalPlanned,
                ["TotalActual"] = x.TotalActual,
                ["ApprovedHours"] = x.ApprovedHours,
            }).ToList();

            return ServiceResult<ReportResultDto>.Ok(new ReportResultDto
            {
                Type = ReportType.OTSummaryByEmployee,
                Title = $"Tổng hợp OT theo nhân viên ({query.FromDate:dd/MM/yyyy} - {query.ToDate:dd/MM/yyyy})",
                TotalRows = rows.Count,
                Columns = new()
                {
                    new() { Field = "EmployeeCode",  Header = "Mã NV" },
                    new() { Field = "EmployeeName",  Header = "Họ tên" },
                    new() { Field = "DeptName",      Header = "Phòng ban" },
                    new() { Field = "RequestCount",  Header = "Số phiếu",     DataType = "number" },
                    new() { Field = "TotalPlanned",  Header = "Giờ kế hoạch", DataType = "decimal", Format = "N1" },
                    new() { Field = "TotalActual",   Header = "Giờ thực tế",  DataType = "decimal", Format = "N1" },
                    new() { Field = "ApprovedHours", Header = "Giờ đã duyệt", DataType = "decimal", Format = "N1" },
                },
                Rows = rows,
                Summary = new()
                {
                    ["TotalEmployees"] = data.Count,
                    ["TotalRequests"] = data.Sum(x => x.RequestCount),
                    ["TotalOTHours"] = data.Sum(x => x.TotalActual),
                    ["ApprovedHours"] = data.Sum(x => x.ApprovedHours),
                },
            });
        }

        // ── OT Detail — nguồn: VF03OTRequest (header, đã đủ EmployeeName/DeptName/OTTypeName) ──
        private async Task<ServiceResult<ReportResultDto>> GetOTDetailAsync(
            ReportQueryDto query, UserIdentityDto user, CancellationToken ct)
        {
            bool isAdmin = user.Permission.IsAdmin();
            bool isManager = user.Permission.IsApprover() || user.LevelApprove > 0;

            var q = _uow.Repository<VF03OTRequest>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true
                         && x.OTDate >= query.FromDate
                         && x.OTDate <= query.ToDate);

            if (!isAdmin && isManager)
                q = q.Where(x => x.DeptCode == user.DeptCode);
            else if (isAdmin && !string.IsNullOrEmpty(query.DeptCode))
                q = q.Where(x => x.DeptCode == query.DeptCode);
            else if (!isAdmin && !isManager)
                q = q.Where(x => x.EmployeeCode == user.EmployeeCode);

            if (!string.IsNullOrEmpty(query.EmployeeCode))
                q = q.Where(x => x.EmployeeCode == query.EmployeeCode);

            var totalCount = await q.CountAsync(ct);

            var data = await q
                .OrderByDescending(x => x.OTDate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(ct);

            var requestIds = data.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToList();
            var stepsMap = await LoadApprovalStepsMapAsync(RequestModule.Overtime, requestIds, ct);

            var rows = data.Select(x => new Dictionary<string, object?>
            {
                ["Id"] = x.Id,
                ["OTDate"] = x.OTDate,
                ["EmployeeCode"] = x.EmployeeCode,
                ["EmployeeName"] = x.EmployeeName ?? x.EmployeeCode,
                ["DeptCode"] = x.DeptCode,
                ["OTTypeCode"] = x.OTTypeName ?? x.OTTypeCode,
                ["TotalOTHours"] = x.TotalOTHours,
                ["OTReasonSummary"] = x.OTReasonSummary,
                ["RequestStatus"] = x.RequestStatus.ToDisplayName(),
                ["Approver_Lv1"] = x.Id.HasValue ? GetApproverName(stepsMap, x.Id.Value, level: 1) : null,
                ["Approver_Lv2"] = x.Id.HasValue ? GetApproverName(stepsMap, x.Id.Value, level: 2) : null,
                ["Approver_Lv3"] = x.Id.HasValue ? GetApproverName(stepsMap, x.Id.Value, level: 3) : null,
            }).ToList();

            return ServiceResult<ReportResultDto>.Ok(new ReportResultDto
            {
                Type = ReportType.OTDetail,
                Title = $"Chi tiết phiếu OT ({query.FromDate:dd/MM/yyyy} - {query.ToDate:dd/MM/yyyy})",
                TotalRows = totalCount,
                Columns = new()
                {
                    new() { Field = "OTDate",          Header = "Ngày OT",     DataType = "date",    Format = "dd/MM/yyyy" },
                    new() { Field = "EmployeeCode",    Header = "Mã NV" },
                    new() { Field = "EmployeeName",    Header = "Họ tên" },
                    new() { Field = "DeptCode",        Header = "Phòng ban" },
                    new() { Field = "OTTypeCode",      Header = "Loại OT" },
                    new() { Field = "TotalOTHours",    Header = "Tổng giờ OT", DataType = "decimal", Format = "N1" },
                    new() { Field = "OTReasonSummary", Header = "Lý do" },
                    new() { Field = "RequestStatus",   Header = "Trạng thái" },
                    new() { Field = "Approver_Lv1",    Header = "Người duyệt cấp 1" },
                    new() { Field = "Approver_Lv2",    Header = "Người duyệt cấp 2" },
                    new() { Field = "Approver_Lv3",    Header = "Người duyệt cấp 3" },
                },
                Rows = rows,
                Summary = new()
                {
                    ["TotalRequests"] = totalCount,
                    ["TotalOTHours"] = data.Sum(x => x.TotalOTHours ?? 0),
                },
            });
        }

        // ── OT Approval Status — nguồn: VF03OTRequest (enum sạch) ──
        private async Task<ServiceResult<ReportResultDto>> GetOTApprovalStatusAsync(
            ReportQueryDto query, UserIdentityDto user, CancellationToken ct)
        {
            var q = _uow.Repository<VF03OTRequest>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true
                         && x.OTDate >= query.FromDate
                         && x.OTDate <= query.ToDate);

            if (!string.IsNullOrEmpty(query.DeptCode))
                q = q.Where(x => x.DeptCode == query.DeptCode);

            var data = await q
                .GroupBy(x => x.RequestStatus)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                    TotalOTHours = g.Sum(x => x.TotalOTHours ?? 0),
                })
                .ToListAsync(ct);

            var rows = data.Select(x => new Dictionary<string, object?>
            {
                ["Status"] = x.Status.ToDisplayName(),
                ["Count"] = x.Count,
                ["TotalOTHours"] = x.TotalOTHours,
            }).ToList();

            return ServiceResult<ReportResultDto>.Ok(new ReportResultDto
            {
                Type = ReportType.OTApprovalStatus,
                Title = "Báo cáo trạng thái phê duyệt OT",
                TotalRows = rows.Count,
                Columns = new()
                {
                    new() { Field = "Status",       Header = "Trạng thái" },
                    new() { Field = "Count",        Header = "Số phiếu",  DataType = "number" },
                    new() { Field = "TotalOTHours", Header = "Tổng giờ",  DataType = "decimal", Format = "N1" },
                },
                Rows = rows,
                Summary = new()
                {
                    ["TotalRequests"] = data.Sum(x => x.Count),
                    ["TotalOTHours"] = data.Sum(x => x.TotalOTHours),
                },
                Chart = new()
                {
                    ChartType = "pie",
                    Labels = data.Select(x => x.Status.ToDisplayName()).ToList(),
                    Series = new()
                    {
                        new() { Name = "Số phiếu", Data = data.Select(x => (decimal)x.Count).ToList() }
                    }
                }
            });
        }

        // ── OT Limit Usage — nguồn: VF03OTRequestDetail (ActualHours theo từng NV/ngày) ──
        private async Task<ServiceResult<ReportResultDto>> GetOTLimitUsageAsync(
            ReportQueryDto query, UserIdentityDto user, CancellationToken ct)
        {
            bool isAdmin = user.Permission.IsAdmin();
            bool isManager = user.Permission.IsApprover() || user.LevelApprove > 0;
            var year = query.WorkYear ?? DateTime.Now.Year;

            var q = _uow.Repository<VF03OTRequestDetail>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true
                         && x.OTDate.Year == year
                         && x.RequestStatus == ApprovalStatus.Approved);

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

            var accumulated = await q
                .GroupBy(x => new { x.EmployeeCode, x.EmployeeName, x.DeptCode, x.DeptName })
                .Select(g => new
                {
                    g.Key.EmployeeCode,
                    g.Key.EmployeeName,
                    g.Key.DeptCode,
                    g.Key.DeptName,
                    UsedHours = g.Sum(x => x.ActualHours ?? x.OTHours),
                })
                .ToListAsync(ct);

            var limits = await _uow.Repository<F03OTLimitRule>().Query()
                .AsNoTracking()
                .Where(r => r.IsActive == true && r.LimitType == OTLimitType.Yearly)
                .ToListAsync(ct);

            decimal GetLimit(string? deptCode)
            {
                return limits
                    .Where(r => r.DeptCode == deptCode || r.DeptCode == "ALL" || r.DeptCode == null)
                    .OrderByDescending(r => r.DeptCode == deptCode)
                    .Select(r => r.LimitValue)
                    .FirstOrDefault(200m);
            }

            var rows = accumulated.Select(x =>
            {
                var limit = GetLimit(x.DeptCode);
                var remain = Math.Max(limit - x.UsedHours, 0);
                var percent = limit > 0 ? Math.Round(x.UsedHours / limit * 100, 1) : 0;

                return new Dictionary<string, object?>
                {
                    ["EmployeeCode"] = x.EmployeeCode,
                    ["EmployeeName"] = x.EmployeeName ?? x.EmployeeCode,
                    ["DeptName"] = x.DeptName ?? x.DeptCode ?? "",
                    ["UsedHours"] = x.UsedHours,
                    ["LimitHours"] = limit,
                    ["RemainHours"] = remain,
                    ["UsedPercent"] = percent,
                    ["IsOverLimit"] = x.UsedHours > limit ? "⚠️ Vượt hạn" : "OK",
                };
            })
            .OrderByDescending(r => r["UsedHours"])
            .ToList();

            return ServiceResult<ReportResultDto>.Ok(new ReportResultDto
            {
                Type = ReportType.OTLimitUsage,
                Title = $"Báo cáo tích lũy giờ OT năm {year}",
                TotalRows = rows.Count,
                Columns = new()
                {
                    new() { Field = "EmployeeCode", Header = "Mã NV" },
                    new() { Field = "EmployeeName", Header = "Họ tên" },
                    new() { Field = "DeptName",     Header = "Phòng ban" },
                    new() { Field = "UsedHours",    Header = "Đã dùng (h)",  DataType = "decimal", Format = "N1" },
                    new() { Field = "LimitHours",   Header = "Giới hạn (h)", DataType = "decimal", Format = "N1" },
                    new() { Field = "RemainHours",  Header = "Còn lại (h)",  DataType = "decimal", Format = "N1" },
                    new() { Field = "UsedPercent",  Header = "% Sử dụng",    DataType = "decimal", Format = "N1" },
                    new() { Field = "IsOverLimit",  Header = "Tình trạng" },
                },
                Rows = rows,
                Summary = new()
                {
                    ["TotalEmployees"] = rows.Count,
                    ["TotalOTHours"] = accumulated.Sum(x => x.UsedHours),
                },
                Chart = new()
                {
                    ChartType = "bar",
                    Labels = rows.Take(10).Select(r => r["EmployeeCode"]?.ToString() ?? "").ToList(),
                    Series = new()
                    {
                        new() { Name = "Đã dùng",  Data = rows.Take(10).Select(r => (decimal)(r["UsedHours"]  ?? 0m)).ToList() },
                        new() { Name = "Giới hạn", Data = rows.Take(10).Select(r => (decimal)(r["LimitHours"] ?? 0m)).ToList() },
                    }
                }
            });
        }
    }
}