using FVN_REGISTER.Application.Interfaces.Companies;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Core.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Companies
{
    public class CompanyHolidayManagementService
        : BaseService<CompanyHolidayManagementService>, ICompanyHolidayManagementService
    {
        private readonly IUnitOfWork _uow;

        public CompanyHolidayManagementService(
            IUnitOfWork uow,
            ILogger<CompanyHolidayManagementService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _uow = uow;
        }

        public async Task<List<CompanyHolidayDto>> GetAllAsync(CancellationToken ct = default)
            => await _uow.Repository<F03CompanyHoliday>().Query().AsNoTracking()
                .OrderByDescending(x => x.HolidayDate).Select(ToDto).ToListAsync(ct);

        public async Task<CompanyHolidayDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _uow.Repository<F03CompanyHoliday>().GetByIdAsync(id, ct);
            return entity == null ? null : ToDto(entity);
        }

        public async Task<List<int>> GetWorkYearsAsync(CancellationToken ct = default)
            => await _uow.Repository<F03WorkYear>().Query().AsNoTracking()
                .Select(x => x.WorkYear).Distinct().OrderByDescending(x => x).ToListAsync(ct);

        public async Task<ServiceResult<CompanyHolidayDto>> CreateAsync(
            CompanyHolidayDto model, int userId, CancellationToken ct = default)
        {
            try
            {
                var repo = _uow.Repository<F03CompanyHoliday>();
                var exists = await repo.Query().AnyAsync(x => x.HolidayDate.Date == model.HolidayDate.Date, ct);
                if (exists) return ServiceResult<CompanyHolidayDto>.Fail("Ngày nghỉ này đã tồn tại.");

                var entity = new F03CompanyHoliday
                {
                    HolidayDate = model.HolidayDate,
                    Description = model.Description,
                    Year = model.HolidayDate.Year,
                    IsPaidLeave = model.IsPaidLeave,
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
            CompanyHolidayDto model, int userId, CancellationToken ct = default)
        {
            try
            {
                var repo = _uow.Repository<F03CompanyHoliday>();
                var entity = await repo.GetByIdAsync(model.Id, ct);
                if (entity == null) return ServiceResult<CompanyHolidayDto>.Fail("Không tìm thấy thông tin.");

                entity.HolidayDate = model.HolidayDate;
                entity.Description = model.Description;
                entity.Year = model.HolidayDate.Year;
                entity.IsPaidLeave = model.IsPaidLeave;
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
            try
            {
                var repo = _uow.Repository<F03CompanyHoliday>();
                var entity = await repo.GetByIdAsync(id, ct);
                if (entity == null) return ServiceResult.Fail("Không tìm thấy.");

                repo.Remove(entity);
                await _uow.SaveChangesAsync(ct);
                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[HOLIDAY] Delete error: {Id}", id);
                return ServiceResult.Fail("Lỗi xóa.");
            }
        }

        public async Task<ServiceResult> CreateSundaysAsync(
            int year, int userId, CancellationToken ct = default)
        {
            var repo = _uow.Repository<F03CompanyHoliday>();
            var sundays = GetSundays(year);
            int addedCount = 0;

            foreach (var date in sundays)
            {
                bool exists = await repo.Query().AnyAsync(x => x.HolidayDate.Date == date.Date, ct);
                if (!exists)
                {
                    await repo.AddAsync(new F03CompanyHoliday
                    {
                        HolidayDate = date,
                        Description = "Chủ nhật",
                        Year = year,
                        IsPaidLeave = false,
                        CreatedBy = userId,
                        CreatedAt = DateTime.Now
                    }, ct);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                await _uow.SaveChangesAsync(ct);
                return ServiceResult.Ok($"Đã thêm {addedCount} ngày chủ nhật.");
            }

            return ServiceResult.Fail("Dữ liệu chủ nhật đã đầy đủ.");
        }

        private static CompanyHolidayDto ToDto(F03CompanyHoliday e) => new()
        {
            Id = e.Id,
            HolidayDate = e.HolidayDate,
            Description = e.Description,
            Year = e.Year,
            IsPaidLeave = e.IsPaidLeave
        };

        private static List<DateTime> GetSundays(int year)
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
