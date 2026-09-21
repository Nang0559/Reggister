using ClosedXML.Excel;
using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Companies;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Companies;

public class CompanyHolidayManagementService : BaseService<CompanyHolidayManagementService>, ICompanyHolidayManagementService
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly IUnitOfWork _uow;

    public CompanyHolidayManagementService(
        IUnitOfWork uow,
        ILogger<CompanyHolidayManagementService> logger,
        IOptionsMonitor<AuthDebugOptions> options)
        : base(logger, options) => _uow = uow;

    public async Task<List<CompanyHolidayDto>> GetAllAsync(CancellationToken ct = default)
        => await _uow.Repository<F03CompanyHoliday>().Query()
            .AsNoTracking()
            .OrderByDescending(x => x.HolidayDate)
            .Select(x => ToDto(x))
            .ToListAsync(ct);

    public async Task<CompanyHolidayDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _uow.Repository<F03CompanyHoliday>().GetByIdAsync(id, ct);
        return entity == null ? null : ToDto(entity);
    }

    public async Task<List<int>> GetWorkYearsAsync(CancellationToken ct = default)
        => await _uow.Repository<F03WorkYear>().Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true)
            .Select(x => x.WorkYear)
            .Distinct()
            .OrderByDescending(x => x)
            .ToListAsync(ct);

    public async Task<ServiceResult<CompanyHolidayDto>> CreateAsync(
        CompanyHolidayDto model,
        int userId,
        CancellationToken ct = default)
    {
        try
        {
            var repo = _uow.Repository<F03CompanyHoliday>();

            if (await repo.Query().AnyAsync(x => x.HolidayDate.Date == model.HolidayDate.Date, ct))
                return ServiceResult<CompanyHolidayDto>.Fail("Ngày nghỉ này đã tồn tại.");

            if (string.IsNullOrWhiteSpace(model.Description))
                return ServiceResult<CompanyHolidayDto>.Fail("Mô tả ngày nghỉ không được trống.");

            var entity = new F03CompanyHoliday
            {
                HolidayDate = model.HolidayDate.Date,
                Description = model.Description.Trim(),
                Year = model.HolidayDate.Year,
                TinhPhep = model.IsPaidLeave,
                CreatedBy = userId,
                CreatedAt = DateTime.Now
            };

            await repo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            return ServiceResult<CompanyHolidayDto>.Ok(ToDto(entity));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[HOLIDAY] Create error");
            return ServiceResult<CompanyHolidayDto>.Fail("Lỗi hệ thống khi tạo ngày nghỉ.");
        }
    }

    public async Task<ServiceResult<CompanyHolidayDto>> UpdateAsync(
        CompanyHolidayDto model,
        int userId,
        CancellationToken ct = default)
    {
        try
        {
            var repo = _uow.Repository<F03CompanyHoliday>();
            var entity = await repo.GetByIdAsync(model.Id, ct);

            if (entity == null)
                return ServiceResult<CompanyHolidayDto>.Fail("Không tìm thấy thông tin.");

            if (string.IsNullOrWhiteSpace(model.Description))
                return ServiceResult<CompanyHolidayDto>.Fail("Mô tả ngày nghỉ không được trống.");

            if (await repo.Query().AnyAsync(
                    x => x.Id != model.Id && x.HolidayDate.Date == model.HolidayDate.Date, ct))
                return ServiceResult<CompanyHolidayDto>.Fail("Ngày nghỉ này đã tồn tại.");

            entity.HolidayDate = model.HolidayDate.Date;
            entity.Description = model.Description.Trim();
            entity.Year = model.HolidayDate.Year;
            entity.TinhPhep = model.IsPaidLeave;
            entity.ModifiedBy = userId;
            entity.ModifiedAt = DateTime.Now;

            await _uow.SaveChangesAsync(ct);
            return ServiceResult<CompanyHolidayDto>.Ok(ToDto(entity));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[HOLIDAY] Update error: {Id}", model.Id);
            return ServiceResult<CompanyHolidayDto>.Fail("Lỗi cập nhật.");
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var repo = _uow.Repository<F03CompanyHoliday>();
        var entity = await repo.GetByIdAsync(id, ct);

        if (entity == null)
            return ServiceResult.Fail("Không tìm thấy.");

        repo.Remove(entity);
        await _uow.SaveChangesAsync(ct);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> CreateSundaysAsync(
        int year,
        int userId,
        CancellationToken ct = default)
    {
        var repo = _uow.Repository<F03CompanyHoliday>();
        var date = new DateTime(year, 1, 1);

        while (date.DayOfWeek != DayOfWeek.Sunday)
            date = date.AddDays(1);

        var count = 0;

        while (date.Year == year)
        {
            if (!await repo.Query().AnyAsync(x => x.HolidayDate.Date == date.Date, ct))
            {
                await repo.AddAsync(new F03CompanyHoliday
                {
                    HolidayDate = date,
                    Description = "Chủ nhật",
                    Year = year,
                    TinhPhep = false,
                    CreatedBy = userId,
                    CreatedAt = DateTime.Now
                }, ct);

                count++;
            }

            date = date.AddDays(7);
        }

        if (count > 0)
        {
            await _uow.SaveChangesAsync(ct);
            return ServiceResult.Ok($"Đã thêm {count} ngày chủ nhật.");
        }

        return ServiceResult.Fail("Dữ liệu chủ nhật đã đầy đủ.");
    }

    public Task<ServiceResult<byte[]>> DownloadTemplateAsync(CancellationToken ct = default)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            using var workbook = new XLWorkbook();

            var ws = workbook.Worksheets.Add("CompanyHolidays");
            ws.Cell(1, 1).Value = "HolidayDate";
            ws.Cell(1, 2).Value = "Description";
            ws.Cell(1, 3).Value = "Year";
            ws.Cell(1, 4).Value = "TinhPhep";

            ws.Cell(2, 1).Value = DateTime.Today;
            ws.Cell(2, 2).Value = "Ví dụ: Tết Dương lịch";
            ws.Cell(2, 3).FormulaA1 = "=YEAR(A2)";
            ws.Cell(2, 4).Value = 1;

            ws.Column(1).Style.DateFormat.Format = "dd/MM/yyyy";
            ws.Column(3).Style.NumberFormat.Format = "0";
            ws.Column(4).Style.NumberFormat.Format = "0";

            var header = ws.Range(1, 1, 1, 4);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.FromHtml("#E8EEF7");
            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range(1, 1, 200, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 200, 4).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Column(1).Width = 16;
            ws.Column(2).Width = 36;
            ws.Column(3).Width = 12;
            ws.Column(4).Width = 12;

            ws.SheetView.FreezeRows(1);
            ws.Range("D2:D200").CreateDataValidation().List("1,0", true);

            var guide = workbook.Worksheets.Add("Hướng dẫn");
            guide.Cell(1, 1).Value = "Mẫu import ngày nghỉ công ty";
            guide.Cell(1, 1).Style.Font.Bold = true;
            guide.Cell(3, 1).Value = "HolidayDate";
            guide.Cell(3, 2).Value = "Ngày nghỉ, định dạng dd/MM/yyyy.";
            guide.Cell(4, 1).Value = "Description";
            guide.Cell(4, 2).Value = "Mô tả ngày nghỉ, bắt buộc.";
            guide.Cell(5, 1).Value = "Year";
            guide.Cell(5, 2).Value = "Năm của HolidayDate. Phải khớp với YEAR(HolidayDate).";
            guide.Cell(6, 1).Value = "TinhPhep";
            guide.Cell(6, 2).Value = "1 = tính phép, 0 = không tính phép.";
            guide.Cell(8, 1).Value = "Lưu ý";
            guide.Cell(8, 2).Value = "Không nhập Id, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt.";
            guide.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return Task.FromResult(ServiceResult<byte[]>.Ok(stream.ToArray()));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[HOLIDAY] Create Excel template error");
            return Task.FromResult(ServiceResult<byte[]>.Fail("Không tạo được mẫu Excel."));
        }
    }

    public async Task<ServiceResult<string>> ImportExcelAsync(
        Stream content,
        string fileName,
        int userId,
        CancellationToken ct = default)
    {
        try
        {
            using var workbook = new XLWorkbook(content);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
                return ServiceResult<string>.Fail("File Excel không có worksheet.");

            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
            if (lastRow < 2)
                return ServiceResult<string>.Fail("File Excel không có dữ liệu.");

            var paidColumn = DetectPaidColumn(worksheet);
            var yearColumn = paidColumn == 4 ? 3 : 0;

            var repo = _uow.Repository<F03CompanyHoliday>();
            var importedDates = new HashSet<DateTime>();
            var added = 0;
            var skipped = 0;

            for (var row = 2; row <= lastRow; row++)
            {
                ct.ThrowIfCancellationRequested();

                var dateCell = worksheet.Cell(row, 1);
                var descCell = worksheet.Cell(row, 2);

                if (dateCell.IsEmpty() && descCell.IsEmpty())
                    continue;

                if (!dateCell.TryGetValue<DateTime>(out var date))
                    return ServiceResult<string>.Fail($"Dòng {row}: cột HolidayDate không hợp lệ.");

                date = date.Date;

                if (yearColumn > 0)
                {
                    var yearText = worksheet.Cell(row, yearColumn).GetString().Trim();

                    if (!int.TryParse(yearText, out var year))
                    {
                        if (!worksheet.Cell(row, yearColumn).TryGetValue<int>(out year))
                            return ServiceResult<string>.Fail($"Dòng {row}: cột Year không hợp lệ.");
                    }

                    if (year != date.Year)
                        return ServiceResult<string>.Fail(
                            $"Dòng {row}: Year ({year}) không khớp với HolidayDate ({date:dd/MM/yyyy}).");
                }

                var description = descCell.GetString().Trim();
                if (string.IsNullOrWhiteSpace(description))
                    return ServiceResult<string>.Fail($"Dòng {row}: thiếu Description.");

                if (!importedDates.Add(date))
                    return ServiceResult<string>.Fail($"Dòng {row}: HolidayDate {date:dd/MM/yyyy} bị trùng trong file.");

                var paidText = worksheet.Cell(row, paidColumn).GetString().Trim();
                var paid = string.IsNullOrWhiteSpace(paidText)
                    ? true
                    : ParseBoolStrict(paidText, row);

                if (await repo.Query().AnyAsync(x => x.HolidayDate.Date == date, ct))
                {
                    skipped++;
                    continue;
                }

                await repo.AddAsync(new F03CompanyHoliday
                {
                    HolidayDate = date,
                    Description = description,
                    Year = date.Year,
                    TinhPhep = paid,
                    CreatedBy = userId,
                    CreatedAt = DateTime.Now
                }, ct);

                added++;
            }

            if (added > 0)
                await _uow.SaveChangesAsync(ct);

            return ServiceResult<string>.Ok(
                $"Import {fileName}: thêm {added}, bỏ qua {skipped} ngày đã tồn tại.");
        }
        catch (FormatException ex)
        {
            Logger.LogWarning(ex, "[HOLIDAY] Invalid Excel value: {FileName}", fileName);
            return ServiceResult<string>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[HOLIDAY] Import Excel error: {FileName}", fileName);
            return ServiceResult<string>.Fail(
                "Không đọc được file Excel. Hãy dùng nút 'Tải mẫu chuẩn' để lấy đúng định dạng.");
        }
    }

    private static int DetectPaidColumn(IXLWorksheet worksheet)
    {
        var c3 = worksheet.Cell(1, 3).GetString().Trim();
        var c4 = worksheet.Cell(1, 4).GetString().Trim();

        if (c4.Contains("TinhPhep", StringComparison.OrdinalIgnoreCase) ||
            c4.Contains("Tính phép", StringComparison.OrdinalIgnoreCase))
            return 4;

        if (c3.Contains("TinhPhep", StringComparison.OrdinalIgnoreCase) ||
            c3.Contains("Tính phép", StringComparison.OrdinalIgnoreCase))
            return 3;

        // Backward compatibility with the old 3-column format: A=Ngày, B=Mô tả, C=Tính phép.
        return 3;
    }

    private static bool ParseBoolStrict(string value, int row)
        => value.Trim().ToLowerInvariant() switch
        {
            "1" or "true" or "yes" or "y" or "x" or "có" => true,
            "0" or "false" or "no" or "n" or "không" => false,
            _ => throw new FormatException(
                $"Dòng {row}: TinhPhep chỉ nhận 1/0 hoặc Có/Không.")
        };

    private static CompanyHolidayDto ToDto(F03CompanyHoliday entity)
        => new()
        {
            Id = entity.Id,
            HolidayDate = entity.HolidayDate,
            Description = entity.Description,
            Year = entity.Year,
            IsPaidLeave = entity.TinhPhep
        };
}