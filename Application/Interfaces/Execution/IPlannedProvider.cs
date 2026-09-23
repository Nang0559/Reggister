namespace FVN_REGISTER.Application.Interfaces.Execution;

public interface IPlannedProvider
{
    string ModuleCode { get; }
    Task ReconcilePlannedAsync(DateOnly from, DateOnly to, byte reconciliationMode, CancellationToken ct);
}
