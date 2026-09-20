namespace FVN_REGISTER.Application.Interfaces.Payroll;
public interface IPayrollInputService
{
 Task<int> PrepareAsync(int periodId, CancellationToken ct=default);
 Task LockAsync(int periodId, CancellationToken ct=default);
}
