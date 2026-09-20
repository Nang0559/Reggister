using FVN_REGISTER.Application.Interfaces.HrmSync;
using FVN_REGISTER.Core.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Data;

namespace FVN_REGISTER.Infrastructure.Services.HrmSync;

public sealed class HrmAttendanceExcelExportService : IHrmAttendanceExcelExportService
{
    private readonly IUnitOfWork _uow;
    private readonly IWebHostEnvironment _environment;

    public HrmAttendanceExcelExportService(IUnitOfWork uow, IWebHostEnvironment environment)
    {
        _uow = uow;
        _environment = environment;
    }

    public Task<ServiceResult<byte[]>> ExportAttendanceAsync(Guid batchId, CancellationToken ct = default)
        => ExportAsync(batchId, false, ct);

    public Task<ServiceResult<byte[]>> ExportOtAsync(Guid batchId, CancellationToken ct = default)
        => ExportAsync(batchId, true, ct);

    private async Task<ServiceResult<byte[]>> ExportAsync(Guid batchId, bool ot, CancellationToken ct)
    {
        try
        {
            _uow.SetCommandTimeout(120);

            var p = new SqlParameter("@BatchId", SqlDbType.UniqueIdentifier) { Value = batchId };

            if (ot)
            {
                var rows = await _uow.SqlQueryRawAsync<OtRow>(@"
SELECT a.WorkDate,a.EmployeeCode,a.FullName,a.DeptCode,a.ShiftAbbr,
       o.RecognizedOTMinutes AS OTRecognizedMinutesDay,
       0 AS OTRecognizedMinutesNight,
       o.ActualOTDayMinutes AS OTMinutesDay,
       o.ActualOTNightMinutes AS OTMinutesNight,
       a.HrmHoliday,a.HrmEmployeeHoliday,a.OtDisplayValue
FROM dbo.F03HrmOTActual o
INNER JOIN dbo.F03HrmAttendanceCalculated a
    ON a.Id=o.SourceAttendanceId
   AND a.CalculationBatchId=o.CalculationBatchId
WHERE o.CalculationBatchId=@BatchId
ORDER BY a.DeptCode,a.EmployeeCode,a.WorkDate;", ct, p);

                if (rows.Count == 0)
                    return ServiceResult<byte[]>.Fail("Không có dữ liệu tính OT của batch này.");

                return ServiceResult<byte[]>.Ok(BuildOtWorkbook(rows));
            }

            var attendance = await _uow.SqlQueryRawAsync<AttendanceRow>(@"
SELECT WorkDate,EmployeeCode,FullName,DeptCode,ShiftAbbr,
       WorkMinutesDay,WorkMinutesNight,
       CheckInTime,CheckOutTime,
       LeaveTotal,LeaveTypeCode,LeaveReason,
       HrmHoliday,HrmEmployeeHoliday,IsLocked,
       AttendanceDisplayValue
FROM dbo.F03HrmAttendanceCalculated
WHERE CalculationBatchId=@BatchId
ORDER BY DeptCode,EmployeeCode,WorkDate;", ct, p);

            if (attendance.Count == 0)
                return ServiceResult<byte[]>.Fail("Không có dữ liệu chấm công của batch này.");

            return ServiceResult<byte[]>.Ok(BuildAttendanceWorkbook(attendance));
        }
        catch (Exception ex)
        {
            return ServiceResult<byte[]>.Fail($"Xuất Excel thất bại: {ex.Message}");
        }
        finally
        {
            _uow.SetCommandTimeout(30);
        }
    }

    private byte[] BuildAttendanceWorkbook(List<AttendanceRow> rows)
    {
        using var input = File.OpenRead(GetTemplate("GA.xls"));
        var workbook = new HSSFWorkbook(input);
        var sheet = workbook.GetSheetAt(0);

        var month = rows.Max(x => x.WorkDate);
        ApplyAttendanceHeader(sheet, month);

        var grouped = rows
            .GroupBy(x => new { x.EmployeeCode, x.FullName, x.DeptCode })
            .OrderBy(x => x.Key.DeptCode)
            .ThenBy(x => x.Key.EmployeeCode)
            .ToList();

        var firstDataRow = 5;
        EnsureRows(sheet, firstDataRow, grouped.Count, 38);

        for (var i = 0; i < grouped.Count; i++)
        {
            var row = sheet.GetRow(firstDataRow + i) ?? sheet.CreateRow(firstDataRow + i);
            var item = grouped[i];

            SetText(row, 0, (i + 1).ToString());
            SetText(row, 1, item.Key.EmployeeCode);
            SetText(row, 2, item.Key.FullName?.Trim());
            SetText(row, 5, GetPosition(item));
            SetText(row, 6, item.Key.DeptCode);

            var byDate = item.ToDictionary(x => x.WorkDate.Date);
            for (var day = 0; day < 31; day++)
            {
                var date = month.AddMonths(-1).Date.AddDays(20 + day);
                var cell = row.GetCell(7 + day) ?? row.CreateCell(7 + day);
                cell.SetCellValue(GetAttendanceMark(byDate.GetValueOrDefault(date)));
            }
        }

        ClearUnusedRows(sheet, firstDataRow + grouped.Count, 38);
        return WriteWorkbook(workbook);
    }

    private byte[] BuildOtWorkbook(List<OtRow> rows)
    {
        using var input = File.OpenRead(GetTemplate("OT - GA.xls"));
        var workbook = new HSSFWorkbook(input);
        var sheet = workbook.GetSheetAt(0);

        var month = rows.Max(x => x.WorkDate);
        ApplyOtHeader(sheet, month);

        var grouped = rows
            .GroupBy(x => new { x.EmployeeCode, x.FullName, x.DeptCode })
            .OrderBy(x => x.Key.DeptCode)
            .ThenBy(x => x.Key.EmployeeCode)
            .ToList();

        var firstDataRow = 6;
        EnsureRows(sheet, firstDataRow, grouped.Count, 35);

        for (var i = 0; i < grouped.Count; i++)
        {
            var row = sheet.GetRow(firstDataRow + i) ?? sheet.CreateRow(firstDataRow + i);
            var item = grouped[i];

            SetText(row, 0, (i + 1).ToString());
            SetText(row, 1, item.Key.EmployeeCode);
            SetText(row, 2, item.Key.FullName?.Trim());
            SetText(row, 3, item.Key.DeptCode);

            var byDate = item.ToDictionary(x => x.WorkDate.Date);
            for (var day = 0; day < 31; day++)
            {
                var date = month.AddMonths(-1).Date.AddDays(20 + day);
                var cell = row.GetCell(4 + day) ?? row.CreateCell(4 + day);

                var value = GetOtValue(byDate.GetValueOrDefault(date));
                cell.SetCellValue(value);
            }
        }

        ClearUnusedRows(sheet, firstDataRow + grouped.Count, 35);
        return WriteWorkbook(workbook);
    }

    private static string GetAttendanceMark(AttendanceRow? row)
    {
        // The calculation SQL owns HRM display semantics. The exporter only renders it.
        return row?.AttendanceDisplayValue?.Trim() ?? string.Empty;
    }

    private static string GetOtValue(OtRow? row)
    {
        // Never infer OT meaning from weekday/holiday in the Excel layer.
        // HRM-compatible calculation persists the display value explicitly.
        return row?.OtDisplayValue?.Trim() ?? string.Empty;
    }

    private static string FormatOtHours(int minutes)
    {
        var hours = Math.Round(minutes / 60d, 2, MidpointRounding.AwayFromZero);
        return hours.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))?.Trim() ?? string.Empty;

    private string GetTemplate(string name)
    {
        var path = Path.Combine(_environment.ContentRootPath, "Templates", "HRM", name);
        if (!File.Exists(path))
            path = Path.Combine(AppContext.BaseDirectory, "Templates", "HRM", name);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Không tìm thấy template Excel: {name}", path);

        return path;
    }

    private static void ApplyAttendanceHeader(ISheet sheet, DateTime month)
    {
        SetText(sheet.GetRow(0), 0, "CÔNG TY TNHH FCC VIỆT NAM");
        SetText(sheet.GetRow(1), 0, $"THÁNG {month:M}");
        SetText(sheet.GetRow(3), 0, $"BẢNG CHẤM CÔNG THÁNG {month:M}/{month:yyyy}");

        for (var i = 0; i < 31; i++)
        {
            var date = month.AddMonths(-1).Date.AddDays(20 + i);
            SetText(sheet.GetRow(3), 7 + i, date.Day == 1 ? date.ToString("dd/MM") : date.Day.ToString());
            SetText(sheet.GetRow(4), 7 + i, date.ToString("ddd", new System.Globalization.CultureInfo("en-US")));
        }
    }

    private static void ApplyOtHeader(ISheet sheet, DateTime month)
    {
        SetText(sheet.GetRow(0), 0, "CÔNG TY TNHH FCC VIỆT NAM");
        SetText(sheet.GetRow(1), 0, $"                BẢNG  LÀM THÊM THÁNG {month:M} NĂM {month:yyyy}");

        for (var i = 0; i < 31; i++)
        {
            var date = month.AddMonths(-1).Date.AddDays(20 + i);
            SetText(sheet.GetRow(3), 4 + i, date.Day == 1 ? date.ToString("dd/MM") : date.Day.ToString());
            SetText(sheet.GetRow(4), 4 + i, date.ToString("ddd", new System.Globalization.CultureInfo("en-US")));
        }
    }

    private static string GetPosition(IEnumerable<AttendanceRow> rows)
        => string.Empty;

    private static void EnsureRows(ISheet sheet, int firstRow, int count, int columns)
    {
        var template = sheet.GetRow(firstRow);
        for (var i = 0; i < count; i++)
        {
            var row = sheet.GetRow(firstRow + i) ?? sheet.CreateRow(firstRow + i);
            if (template is not null && row != template)
            {
                row.Height = template.Height;
                for (var c = 0; c < columns; c++)
                {
                    var source = template.GetCell(c);
                    if (source is null) continue;
                    var target = row.GetCell(c) ?? row.CreateCell(c);
                    target.CellStyle = source.CellStyle;
                }
            }
        }
    }

    private static void ClearUnusedRows(ISheet sheet, int start, int columns)
    {
        for (var r = start; r <= sheet.LastRowNum; r++)
        {
            var row = sheet.GetRow(r);
            if (row is null) continue;
            for (var c = 0; c < columns; c++)
                row.GetCell(c)?.SetCellValue(string.Empty);
        }
    }

    private static void SetText(IRow? row, int index, string? value)
    {
        if (row is null) return;
        var cell = row.GetCell(index) ?? row.CreateCell(index);
        cell.SetCellValue(value ?? string.Empty);
    }

    private static byte[] WriteWorkbook(HSSFWorkbook workbook)
    {
        using var output = new MemoryStream();
        workbook.Write(output, false);
        return output.ToArray();
    }

    private sealed class AttendanceRow
    {
        public DateTime WorkDate { get; set; }
        public string? EmployeeCode { get; set; }
        public string? FullName { get; set; }
        public string? DeptCode { get; set; }
        public string? ShiftAbbr { get; set; }
        public int WorkMinutesDay { get; set; }
        public int WorkMinutesNight { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public decimal? LeaveTotal { get; set; }
        public string? LeaveTypeCode { get; set; }
        public string? LeaveReason { get; set; }
        public bool? HrmHoliday { get; set; }
        public bool? HrmEmployeeHoliday { get; set; }
        public bool? IsLocked { get; set; }
        public string? AttendanceDisplayValue { get; set; }
    }

    private sealed class OtRow
    {
        public DateTime WorkDate { get; set; }
        public string? EmployeeCode { get; set; }
        public string? FullName { get; set; }
        public string? DeptCode { get; set; }
        public string? ShiftAbbr { get; set; }
        public int OTRecognizedMinutesDay { get; set; }
        public int OTRecognizedMinutesNight { get; set; }
        public int OTMinutesDay { get; set; }
        public int OTMinutesNight { get; set; }
        public bool? HrmHoliday { get; set; }
        public bool? HrmEmployeeHoliday { get; set; }
        public string? OtDisplayValue { get; set; }
    }
}