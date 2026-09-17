using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Infrastructure;
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

            var holidays = await _cache.GetOrCreateAsync(HOLIDAY_CACHE_KEY, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);

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

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                if (holidays != null && holidays.Contains(date))
                    continue;

                count++;
            }

            return count;
        }

        public async Task<double> GetWorkingHoursAsync(DateTime from, DateTime to)
        {
            if (from >= to) return 0;

            var holidays = await GetHolidaysAsync();
            double totalHours = 0;

            for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday || holidays.Contains(date))
                    continue;

                DateTime workStart = date.AddHours(8);
                DateTime workEnd = date.AddHours(17);
                DateTime lunchStart = date.AddHours(12);
                DateTime lunchEnd = date.AddHours(13);

                DateTime effectiveStart = from > workStart ? from : workStart;
                DateTime effectiveEnd = to < workEnd ? to : workEnd;

                if (effectiveStart < effectiveEnd)
                {
                    double hours = (effectiveEnd - effectiveStart).TotalHours;

                    if (effectiveStart < lunchEnd && effectiveEnd > lunchStart)
                    {
                        long overlapStart = Math.Max(effectiveStart.Ticks, lunchStart.Ticks);
                        long overlapEnd = Math.Min(effectiveEnd.Ticks, lunchEnd.Ticks);
                        hours -= TimeSpan.FromTicks(overlapEnd - overlapStart).TotalHours;
                    }

                    totalHours += hours;
                }
            }

            return Math.Round(totalHours, 2);
        }

        private async Task<HashSet<DateTime>> GetHolidaysAsync()
        {
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
