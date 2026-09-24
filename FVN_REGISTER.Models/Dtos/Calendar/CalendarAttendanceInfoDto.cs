namespace FVN_REGISTER.Contract.Dtos.Calendar;

public sealed class CalendarAttendanceInfoDto
{
    public DateTime? CheckIn { get; init; }
    public DateTime? CheckOut { get; init; }
    public int WorkMinutes { get; init; }
    public int RequiredMinutes { get; init; }
    public decimal? ActualHours { get; init; }
    public decimal? RequiredHours { get; init; }
    public int ActualOtMinutes { get; init; }
    public int RecognizedOtMinutes { get; init; }
    public bool HasActual { get; init; }
    public bool HasActualOt { get; init; }
    public bool IsNumericVariance { get; init; }
    public string? DisplayValue { get; init; }
    public string? SourceId { get; init; }
    public Guid? ActionId { get; init; }
    public string? DetailRoute { get; init; }
}