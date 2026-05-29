

namespace FVN_REGISTER.Contract.Interfaces.Leaves
{
    public interface ILeaveEscalationService
    {
        Task ProcessAutoEscalationAsync(CancellationToken ct = default);
    }
}
