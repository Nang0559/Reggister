using ClosedXML.Excel;
using FVN_REGISTER.Application.Interfaces.Reports;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Reports;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Application.Interfaces.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace FVN_REGISTER.Infrastructure.Services.Reports
{
    public class ReportDispatcher : BaseService<ReportDispatcher>, IReportDispatcher
    {
        private readonly IEnumerable<IReportService> _reportServices;
        private readonly IUnitOfWork _uow;
        private readonly IAuthorizationService _authorization;

        public ReportDispatcher(
            IEnumerable<IReportService> reportServices,
            IUnitOfWork uow,
            IAuthorizationService authorization,
            ILogger<ReportDispatcher> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _authorization = authorization;
            _reportServices = reportServices;
            _uow = uow;
        }

        // ════════════════════════════════════════════════════════════
        // GET REPORT — route theo CanHandle, KHÔNG sửa khi thêm report mới
        // ════════════════════════════════════════════════════════════
        public async Task<ServiceResult<ReportResultDto>> GetReportAsync(
            ReportQueryDto query, UserIdentityDto user, CancellationToken ct = default)
        {
            try
            {
                var requiredFunction = GetViewFunction(query.Type);
                if (requiredFunction == 0 || !await _authorization.HasAsync(user, requiredFunction, ct))
                    return ServiceResult<ReportResultDto>.Fail("Bạn không có quyền xem báo cáo này.");

                if (!await _authorization.CanAccessAsync(user, requiredFunction, query.EmployeeCode, query.DeptCode, ct))
                    return ServiceResult<ReportResultDto>.Fail("Bạn không có quyền truy cập phạm vi dữ liệu báo cáo này.");

                var reportScope = await _authorization.GetScopeAsync(user.UserId, requiredFunction, ct);
                if (string.Equals(reportScope, AuthorizationScopeCodes.Department, StringComparison.OrdinalIgnoreCase))
                {
                    query.DeptCode = user.DeptCode;
                }
                else if (string.Equals(reportScope, AuthorizationScopeCodes.Own, StringComparison.OrdinalIgnoreCase)
                      || string.Equals(reportScope, AuthorizationScopeCodes.Employee, StringComparison.OrdinalIgnoreCase))
                {
                    query.DeptCode = user.DeptCode;
                    query.EmployeeCode = user.EmployeeCode;
                }

                var handler = _reportServices.FirstOrDefault(s => s.CanHandle(query.Type));

                if (handler == null)
                {
                    Logger.LogWarnIf(Debug, "[REPORT] Không tìm thấy handler cho: {Type}", query.Type);
                    return ServiceResult<ReportResultDto>.Fail($"Không hỗ trợ loại báo cáo: {query.Type}");
                }

                Logger.LogDebugIf(Debug, "[REPORT] Dispatch {Type} -> {Handler}", query.Type, handler.GetType().Name);

                return await handler.GetReportAsync(query, user, ct);
            }
            catch (Exception ex)
            {
                return InternalError<ReportResultDto>(ex, "Lỗi hệ thống khi tải báo cáo");
            }
        }

        // ════════════════════════════════════════════════════════════
        // EXPORT EXCEL — dùng lại GetReportAsync, KHÔNG viết lại logic query
        // ════════════════════════════════════════════════════════════
        public async Task<ServiceResult<byte[]>> ExportExcelAsync(
            ReportQueryDto query, UserIdentityDto user, CancellationToken ct = default)
        {
            try
            {
                var exportFunction = GetExportFunction(query.Type);
                if (exportFunction == 0 || !await _authorization.HasAsync(user, exportFunction, ct))
                    return ServiceResult<byte[]>.Fail("Bạn không có quyền xuất báo cáo này.");

                var reportResult = await GetReportAsync(query, user, ct);

                if (!reportResult.IsSuccess || reportResult.Data == null)
                {
                    Logger.LogWarnIf(Debug, "[REPORT] Export thất bại: {Msg}", reportResult.Message);
                    return ServiceResult<byte[]>.Fail(reportResult.Message ?? "Không có dữ liệu để xuất.");
                }

                var report = reportResult.Data;

                using var workbook = new XLWorkbook();
                var sheetName = report.Title.Length > 31 ? report.Title[..31] : report.Title;
                var ws = workbook.Worksheets.Add(string.IsNullOrWhiteSpace(sheetName) ? "Report" : sheetName);

                // Header row
                for (int col = 0; col < report.Columns.Count; col++)
                {
                    var cell = ws.Cell(1, col + 1);
                    cell.Value = report.Columns[col].Header;
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                // Data rows
                for (int rowIdx = 0; rowIdx < report.Rows.Count; rowIdx++)
                {
                    var row = report.Rows[rowIdx];
                    for (int col = 0; col < report.Columns.Count; col++)
                    {
                        var field = report.Columns[col].Field;
                        var value = row.TryGetValue(field, out var v) ? v : null;
                        var cell = ws.Cell(rowIdx + 2, col + 1);

                        SetCellValue(cell, value, report.Columns[col].DataType);
                    }
                }

                ws.Columns().AdjustToContents();
                ws.SheetView.FreezeRows(1);

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);

                Logger.LogInfoIf(Debug, "[REPORT] Export Excel OK: {Type} - {Rows} dòng", query.Type, report.Rows.Count);

                return ServiceResult<byte[]>.Ok(stream.ToArray());
            }
            catch (Exception ex)
            {
                return InternalError<byte[]>(ex, "Lỗi hệ thống khi xuất Excel");
            }
        }

        private static void SetCellValue(IXLCell cell, object? value, string? dataType)
        {
            if (value == null)
            {
                cell.Value = "";
                return;
            }

            switch (dataType)
            {
                case "number":
                    if (value is int i) cell.Value = i;
                    else if (int.TryParse(value.ToString(), out var iv)) cell.Value = iv;
                    else cell.Value = value.ToString();
                    break;

                case "decimal":
                    if (value is decimal dec) cell.Value = dec;
                    else if (decimal.TryParse(value.ToString(), out var dv)) cell.Value = dv;
                    else cell.Value = value.ToString();
                    break;

                case "date":
                    if (value is DateTime dt) cell.Value = dt;
                    else if (value is DateOnly d) cell.Value = d.ToDateTime(TimeOnly.MinValue);
                    else if (DateTime.TryParse(value.ToString(), out var pdt)) cell.Value = pdt;
                    else cell.Value = value.ToString();
                    break;

                default:
                    cell.Value = value.ToString();
                    break;
            }
        }

        // ════════════════════════════════════════════════════════════
        // LOOKUP: DEPARTMENTS — dùng cho dropdown filter trên UI báo cáo
        // ════════════════════════════════════════════════════════════
        public async Task<ServiceResult<List<KeyValuePair<string, string>>>> GetLookupDepartmentsAsync(
            UserIdentityDto user, CancellationToken ct = default)
        {
            try
            {
                if (!await HasAnyReportViewAsync(user, ct))
                    return ServiceResult<List<KeyValuePair<string, string>>>.Fail("Bạn không có quyền xem báo cáo.");
                var list = await _uow.Repository<F03Department>().Query()
                    .AsNoTracking()
                    .Where(x => x.IsActive == true)
                    .OrderBy(x => x.DeptName)
                    .Select(x => new KeyValuePair<string, string>(x.DeptCode, x.DeptName))
                    .ToListAsync(ct);

                Logger.LogDebugIf(Debug, "[REPORT] Lookup Departments: {Count}", list.Count);

                return ServiceResult<List<KeyValuePair<string, string>>>.Ok(list);
            }
            catch (Exception ex)
            {
                return InternalError<List<KeyValuePair<string, string>>>(ex, "Lỗi tải danh sách phòng ban");
            }
        }

        // ════════════════════════════════════════════════════════════
        // LOOKUP: EMPLOYEES — search theo tên/mã, optional filter theo phòng ban
        // ════════════════════════════════════════════════════════════
        public async Task<ServiceResult<List<KeyValuePair<string, string>>>> SearchLookupEmployeesAsync(
            string filterText, UserIdentityDto user,
            string? deptCode = null, CancellationToken ct = default)
        {
            try
            {
                if (!await HasAnyReportViewAsync(user, ct))
                    return ServiceResult<List<KeyValuePair<string, string>>>.Fail("Bạn không có quyền xem báo cáo.");
                var q = _uow.Repository<VF03employee>().Query()
                    .AsNoTracking()
                    .Where(x => x.IsActive);

                if (!string.IsNullOrWhiteSpace(deptCode))
                    q = q.Where(x => x.DeptCode == deptCode);

                if (!string.IsNullOrWhiteSpace(filterText))
                {
                    var keyword = filterText.Trim();
                    q = q.Where(x =>
                        x.EmployeeCode.Contains(keyword) ||
                        x.EmployeeName.Contains(keyword));
                }

                var list = await q
                    .OrderBy(x => x.EmployeeName)
                    .Take(50) // giới hạn kết quả autocomplete, tránh trả về toàn bộ NV
                    .Select(x => new KeyValuePair<string, string>(
                        x.EmployeeCode,
                        x.EmployeeName + " (" + x.EmployeeCode + ")"))
                    .ToListAsync(ct);

                Logger.LogDebugIf(Debug,
                    "[REPORT] Search Employees: keyword={Keyword}, dept={Dept}, count={Count}",
                    filterText, deptCode, list.Count);

                return ServiceResult<List<KeyValuePair<string, string>>>.Ok(list);
            }
            catch (Exception ex)
            {
                return InternalError<List<KeyValuePair<string, string>>>(ex, "Lỗi tìm kiếm nhân viên");
            }
        }
        private async Task<bool> HasAnyReportViewAsync(UserIdentityDto user, CancellationToken ct)
            => await _authorization.HasAsync(user, SecurityFunctionCodes.LeaveView, ct)
            || await _authorization.HasAsync(user, SecurityFunctionCodes.OTView, ct)
            || await _authorization.HasAsync(user, SecurityFunctionCodes.TripView, ct)
            || await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentView, ct)
            || await _authorization.HasAsync(user, SecurityFunctionCodes.AttendanceView, ct);

        private static int GetViewFunction(ReportType type) => type switch
        {
            ReportType.LeaveBalance or ReportType.LeaveSummaryByDept or ReportType.LeaveSummaryByEmployee or ReportType.LeaveDetail or ReportType.LeaveApprovalStatus => SecurityFunctionCodes.LeaveView,
            ReportType.OTSummaryByDept or ReportType.OTSummaryByEmployee or ReportType.OTDetail or ReportType.OTApprovalStatus or ReportType.OTLimitUsage => SecurityFunctionCodes.OTView,
            ReportType.TripSummaryByDept or ReportType.TripSummaryByEmployee or ReportType.TripDetail or ReportType.TripApprovalStatus => SecurityFunctionCodes.TripView,
            ReportType.EquipmentSummaryByDept or ReportType.EquipmentAssetDetail or ReportType.EquipmentRepairSummary => SecurityFunctionCodes.EquipmentView,
            ReportType.AttendanceSummary or ReportType.AttendanceDetail => SecurityFunctionCodes.AttendanceView,
            ReportType.CompanyWorkloadSummary => SecurityFunctionCodes.DashboardView,
            ReportType.SecurityAuditSummary => SecurityFunctionCodes.SecurityAudit,
            _ => 0
        };

        private static int GetExportFunction(ReportType type) => type switch
        {
            ReportType.LeaveBalance or ReportType.LeaveSummaryByDept or ReportType.LeaveSummaryByEmployee or ReportType.LeaveDetail or ReportType.LeaveApprovalStatus => SecurityFunctionCodes.LeaveExport,
            ReportType.OTSummaryByDept or ReportType.OTSummaryByEmployee or ReportType.OTDetail or ReportType.OTApprovalStatus or ReportType.OTLimitUsage => SecurityFunctionCodes.OTExport,
            ReportType.TripSummaryByDept or ReportType.TripSummaryByEmployee or ReportType.TripDetail or ReportType.TripApprovalStatus => SecurityFunctionCodes.TripExport,
            ReportType.EquipmentSummaryByDept or ReportType.EquipmentAssetDetail or ReportType.EquipmentRepairSummary => SecurityFunctionCodes.EquipmentExport,
            ReportType.AttendanceSummary or ReportType.AttendanceDetail => SecurityFunctionCodes.AttendanceExport,
            ReportType.CompanyWorkloadSummary => SecurityFunctionCodes.DashboardView,
            ReportType.SecurityAuditSummary => SecurityFunctionCodes.SecurityAudit,
            _ => 0
        };

    }
}