using FVN_REGISTER.Application.Models.Calendar;
using FVN_REGISTER.Contract.Dtos.Calendar;

namespace FVN_REGISTER.Application.Interfaces.Calendar;

public interface ICalendarModuleProvider
{
    string ModuleCode { get; }

    Task<IReadOnlyList<CalendarItemDto>> GetItemsAsync(
        CalendarContext context,
        CancellationToken cancellationToken = default);
}
