namespace FVN_REGISTER.Application.Interfaces.Payroll;

using FVN_REGISTER.Contract.Dtos.Payroll;

public interface IPayrollInputService
{
    Task<PayrollPeriodDto> GetOrCreateCurrentPeriodAsync(int actorUserId, CancellationToken ct = default);
    Task<IReadOnlyList<PayrollPeriodDto>> GetPeriodsAsync(CancellationToken ct = default);
    Task<PayrollPrepareDto> PrepareAsync(int periodId, int actorUserId, CancellationToken ct = default);
    Task<PayrollPeriodDto> LockAsync(int periodId, int actorUserId, CancellationToken ct = default);
    Task<PayrollExportDto> ExportAsync(int periodId, int actorUserId, CancellationToken ct = default);
    Task<IReadOnlyList<PayrollInputDto>> GetInputsAsync(int periodId, CancellationToken ct = default);
}
