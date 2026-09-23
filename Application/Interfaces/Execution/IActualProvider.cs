namespace FVN_REGISTER.Application.Interfaces.Execution;

public interface IActualProvider
{
    string ModuleCode { get; }
    Task ReconcileActualAsync(DateOnly from, DateOnly to, byte reconciliationMode, CancellationToken ct);
}
