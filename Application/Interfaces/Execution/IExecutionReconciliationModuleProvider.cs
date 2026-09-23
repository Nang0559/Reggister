namespace FVN_REGISTER.Application.Interfaces.Execution;

public interface IExecutionReconciliationModuleProvider
{
    string ModuleCode { get; }
    Task ReconcileAsync(DateOnly from, DateOnly to, byte reconciliationMode, CancellationToken ct);
}
