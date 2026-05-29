using FVN_REGISTER.Contract.Interfaces.Leaves;
using FVN_REGISTER.Contract.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FVN_REGISTER.API.Services.Leaves
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
    }
}
