namespace FVN_REGISTER.Contract.Dtos.Payroll;

public sealed record PayrollPeriodDto(
    int Id,
    string PeriodCode,
    DateOnly FromDate,
    DateOnly ToDate,
    string Status,
    DateTime? CalculatedAt,
    int? CalculatedBy,
    DateTime? LockedAt,
    int? LockedBy,
    DateTime? ExportedAt,
    int? ExportedBy);

public sealed record PayrollPrepareDto(int PeriodId, int InputRows);

public sealed record PayrollInputDto(
    int Id,
    int PayrollPeriodId,
    int EmployeeId,
    string EmployeeCode,
    string? EmployeeName,
    DateOnly WorkDate,
    decimal WorkMinutes,
    decimal LeaveTotal,
    decimal OTMinutes,
    DateTime SnapshotAt);

public sealed record PayrollExportDto(
    int PeriodId,
    string PeriodCode,
    DateTime ExportedAt,
    int RowCount,
    string FileName,
    string ContentType,
    byte[] Content);
