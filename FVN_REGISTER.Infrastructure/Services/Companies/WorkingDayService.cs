using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FVN_REGISTER.Infrastructure.Services.Companies;

public class WorkingDayService : IWorkingDayService
{
    private readonly FVNWEBAPPContext _db;
    private readonly IMemoryCache _cache;
    private const string HolidayCacheKey = "CompanyHolidays_Set";

    public WorkingDayService(FVNWEBAPPContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<int> GetWorkingDaysAsync(DateTime from, DateTime to)
    {
        if (from > to) return 0;

        var holidays = await GetHolidaysAsync();
        int count = 0;

        for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
        {
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                continue;

            if (holidays.Contains(date))
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
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday || holidays.Contains(date))
                continue;

            var workStart = date.AddHours(8);
            var workEnd = date.AddHours(17);
            var lunchStart = date.AddHours(12);
            var lunchEnd = date.AddHours(13);

            var effectiveStart = from > workStart ? from : workStart;
            var effectiveEnd = to < workEnd ? to : workEnd;

            if (effectiveStart < effectiveEnd)
            {
                double hours = (effectiveEnd - effectiveStart).TotalHours;

                if (effectiveStart < lunchEnd && effectiveEnd > lunchStart)
                {
                    long overlapStart = Math.Max(effectiveStart.Ticks, lunchStart.Ticks);
                    long overlapEnd = Math.Min(effectiveEnd.Ticks, lunchEnd.Ticks);
                    if (overlapEnd > overlapStart)
                        hours -= TimeSpan.FromTicks(overlapEnd - overlapStart).TotalHours;
                }

                totalHours += hours;
            }
        }

        return Math.Round(totalHours, 2);
    }

    private async Task<HashSet<DateTime>> GetHolidaysAsync()
    {
        var result = await _cache.GetOrCreateAsync(HolidayCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);

            var list = await _db.CompanyHolidays
                .AsNoTracking()
                .Select(x => x.HolidayDate.Date)
                .ToListAsync();

            return new HashSet<DateTime>(list);
        });

        return result ?? new HashSet<DateTime>();
    }
}
