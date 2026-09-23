namespace FVN_REGISTER.Application.Interfaces.Execution;

public interface IReconciliationMapper
{
    string ModuleCode { get; }
    Task MapAsync(DateOnly from, DateOnly to, byte reconciliationMode, CancellationToken ct);
}
