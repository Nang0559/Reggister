using FVN_REGISTER.Contract.Interfaces.Companies;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.API.Services.Companies
{
    public class CompanyHolidayService
    : BaseService<CompanyHolidayService>, ICompanyHolidayService
    {
        private readonly FVNWEBAPPContext _db;

        public CompanyHolidayService(
            FVNWEBAPPContext db,
            ILogger<CompanyHolidayService> logger,
            IOptionsMonitor<AuthDebugOptions> options
        ) : base(logger, options)
        {
            _db = db;
        }

        public async Task<List<CompanyHoliday>> GetAllAsync()
        {
            return await _db.CompanyHolidays
                .OrderByDescending(x => x.HolidayDate)
                .ToListAsync();
        }

        public async Task<CompanyHoliday?> GetByIdAsync(int id)
        {
            return await _db.CompanyHolidays.FindAsync(id);
        }

        public async Task<List<int>> GetWorkYearsAsync()
        {
            return await _db.F03workYears
                .Where(x => x.WorkYear.HasValue)
                .Select(x => x.WorkYear!.Value)
                .Distinct()
                .OrderByDescending(x => x)
                .ToListAsync();
        }

        public async Task<ServiceResult> CreateAsync(CompanyHoliday model)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[HOLIDAY] Create attempt");

                if (model.HolidayDate == null)
                {
                    Logger.LogWarnIf(Debug, "[HOLIDAY] Date null");
                    return ServiceResult.Fail("Ngày nghỉ không được để trống.");
                }

                var exists = await _db.CompanyHolidays
                    .AnyAsync(x => x.HolidayDate.HasValue &&
                                   x.HolidayDate.Value.Date == model.HolidayDate.Value.Date);

                if (exists)
                {
                    Logger.LogWarnIf(Debug, "[HOLIDAY] Duplicate date: {Date}", model.HolidayDate);
                    return ServiceResult.Fail("Ngày nghỉ này đã tồn tại.");
                }

                model.Year = model.HolidayDate.Value.Year;

                _db.CompanyHolidays.Add(model);
                await _db.SaveChangesAsync();

                Logger.LogInfoIf(Debug, "[HOLIDAY] Created: {Date}", model.HolidayDate);

                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[HOLIDAY] Create error");
                return ServiceResult.Fail("Lỗi hệ thống khi tạo ngày nghỉ.");
            }
        }

        public async Task<ServiceResult> UpdateAsync(CompanyHoliday model)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[HOLIDAY] Update: {Id}", model.Id);

                var entity = await _db.CompanyHolidays.FindAsync(model.Id);

                if (entity == null)
                {
                    Logger.LogWarnIf(Debug, "[HOLIDAY] Not found: {Id}", model.Id);
                    return ServiceResult.Fail("Không tìm thấy thông tin.");
                }

                entity.HolidayDate = model.HolidayDate;
                entity.Description = model.Description;
                entity.TinhPhep = model.TinhPhep;

                if (model.HolidayDate.HasValue)
                    entity.Year = model.HolidayDate.Value.Year;

                await _db.SaveChangesAsync();

                Logger.LogInfoIf(Debug, "[HOLIDAY] Updated: {Id}", model.Id);

                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[HOLIDAY] Update error: {Id}", model.Id);
                return ServiceResult.Fail("Lỗi cập nhật.");
            }
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[HOLIDAY] Delete: {Id}", id);

                var entity = await _db.CompanyHolidays.FindAsync(id);

                if (entity == null)
                {
                    Logger.LogWarnIf(Debug, "[HOLIDAY] Not found: {Id}", id);
                    return ServiceResult.Fail("Không tìm thấy.");
                }

                _db.CompanyHolidays.Remove(entity);
                await _db.SaveChangesAsync();

                Logger.LogInfoIf(Debug, "[HOLIDAY] Deleted: {Id}", id);

                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[HOLIDAY] Delete error: {Id}", id);
                return ServiceResult.Fail("Lỗi xóa.");
            }
        }

        public async Task<ServiceResult> CreateSundaysAsync(int year)
        {
            var sundays = GetSundays(year);
            int addedCount = 0;

            foreach (var date in sundays)
            {
                // So sánh Date để tránh duplicate
                bool exists = await _db.CompanyHolidays
                    .AnyAsync(x => x.HolidayDate.HasValue && x.HolidayDate.Value.Date == date.Date);

                if (!exists)
                {
                    _db.CompanyHolidays.Add(new CompanyHoliday
                    {
                        HolidayDate = date,
                        Description = "Chủ nhật",
                        Year = year,
                        TinhPhep = 0
                    });
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                await _db.SaveChangesAsync();
                return ServiceResult.Ok($"Đã thêm {addedCount} ngày chủ nhật.");
            }

            return ServiceResult.Fail("Dữ liệu chủ nhật đã đầy đủ.");
        }

        private List<DateTime> GetSundays(int year)
        {
            var list = new List<DateTime>();
            var date = new DateTime(year, 1, 1);

            while (date.DayOfWeek != DayOfWeek.Sunday)
                date = date.AddDays(1);

            while (date.Year == year)
            {
                list.Add(date);
                date = date.AddDays(7);
            }

            return list;
        }
    }
}
