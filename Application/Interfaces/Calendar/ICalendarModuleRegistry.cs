namespace FVN_REGISTER.Application.Interfaces.Calendar;

public interface ICalendarModuleRegistry
{
    IReadOnlyList<ICalendarModuleProvider> Providers { get; }

    ICalendarModuleProvider? Get(string moduleCode);
}
