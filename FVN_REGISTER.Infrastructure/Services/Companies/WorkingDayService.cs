using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FVN_REGISTER.Infrastructure.Services.Companies
{
    public class WorkingDayService : IWorkingDayService
    {
        private readonly FVNWEBAPPContext _db;
        private readonly IMemoryCache _cache;
        private const string HOLIDAY_CACHE_KEY = "CompanyHolidays_Set";

        public WorkingDayService(FVNWEBAPPContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<int> GetWorkingDaysAsync(DateTime from, DateTime to)
        {
            if (from > to) return 0;

            // 1. Lấy danh sách ngày nghỉ từ Cache (HashSet để tối ưu tốc độ tìm kiếm)
            var holidays = await _cache.GetOrCreateAsync(HOLIDAY_CACHE_KEY, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1); // Thường ngày lễ chỉ đổi 1 lần/năm

                var list = await _db.CompanyHolidays
                    .AsNoTracking()
                    .Where(x => x.HolidayDate != null)
                    .Select(x => x.HolidayDate.Value.Date)
                    .ToListAsync();

                return new HashSet<DateTime>(list);
            });

            int count = 0;
            DateTime startDate = from.Date;
            DateTime endDate = to.Date;

            // 2. Vòng lặp tính toán
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // Bỏ qua Thứ 7 và Chủ Nhật (Luật chung FCC)
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                // Bỏ qua nếu nằm trong danh sách ngày lễ của công ty
                if (holidays != null && holidays.Contains(date))
                    continue;

                count++;
            }

            // Lưu ý: Thường logic quá hạn sẽ không tính ngày đầu tiên (ngày gửi đơn)
            // Nếu Rotyby muốn tính "Số ngày đã trôi qua" thì có thể count - 1
            return count;
        }
        public async Task<double> GetWorkingHoursAsync(DateTime from, DateTime to)
        {
            if (from >= to) return 0;

            var holidays = await GetHolidaysAsync(); // Lấy từ cache như logic cũ
            double totalHours = 0;

            for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
            {
                // 1. Bỏ qua ngày nghỉ và cuối tuần
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday || holidays.Contains(date))
                    continue;

                // 2. Định nghĩa khung giờ làm việc trong ngày
                DateTime workStart = date.AddHours(8);
                DateTime workEnd = date.AddHours(17);
                DateTime lunchStart = date.AddHours(12);
                DateTime lunchEnd = date.AddHours(13);

                // 3. Xác định khoảng thời gian làm việc trong ngày hiện tại
                DateTime effectiveStart = from > workStart ? from : workStart;
                DateTime effectiveEnd = to < workEnd ? to : workEnd;

                if (effectiveStart < effectiveEnd)
                {
                    double hours = (effectiveEnd - effectiveStart).TotalHours;

                    // 4. Trừ giờ nghỉ trưa nếu khoảng thời gian này nằm đè lên khung 12h-13h
                    if (effectiveStart < lunchEnd && effectiveEnd > lunchStart)
                    {
                        double overlapStart = Math.Max(effectiveStart.Ticks, lunchStart.Ticks);
                        double overlapEnd = Math.Min(effectiveEnd.Ticks, lunchEnd.Ticks);
                        hours -= TimeSpan.FromTicks((long)(overlapEnd - overlapStart)).TotalHours;
                    }

                    totalHours += hours;
                }
            }

            return Math.Round(totalHours, 2); // Trả về số thập phân (ví dụ: 1.5)
        }
        private async Task<HashSet<DateTime>> GetHolidaysAsync()
        {
            // Sử dụng ?? new HashSet<DateTime>() để đảm bảo không bao giờ trả về null
            var result = await _cache.GetOrCreateAsync(HOLIDAY_CACHE_KEY, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);

                var list = await _db.CompanyHolidays
                    .AsNoTracking()
                    .Where(x => x.HolidayDate.HasValue)
                    .Select(x => x.HolidayDate!.Value.Date)
                    .ToListAsync();

                return new HashSet<DateTime>(list);
            });

            return result ?? new HashSet<DateTime>();
        }
    }
}
