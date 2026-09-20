using FVN_REGISTER.Application.Interfaces.Calendar;

namespace FVN_REGISTER.Infrastructure.Services.Calendar;

public sealed class CalendarModuleRegistry : ICalendarModuleRegistry
{
    public CalendarModuleRegistry(IEnumerable<ICalendarModuleProvider> providers)
    {
        Providers = providers
            .GroupBy(x => x.ModuleCode, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToArray();
    }

    public IReadOnlyList<ICalendarModuleProvider> Providers { get; }

    public ICalendarModuleProvider? Get(string moduleCode) =>
        Providers.FirstOrDefault(
            x => string.Equals(x.ModuleCode, moduleCode, StringComparison.OrdinalIgnoreCase));
}
