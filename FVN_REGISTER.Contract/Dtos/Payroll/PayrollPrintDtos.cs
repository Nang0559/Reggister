namespace FVN_REGISTER.Contract.Dtos.Payroll;

public sealed record PayrollPrintResultDto(
    PayrollPeriodDto Period,
    IReadOnlyList<PayrollInputDto> Inputs);
