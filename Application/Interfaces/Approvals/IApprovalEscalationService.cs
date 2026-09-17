

namespace FVN_REGISTER.Application.Interfaces.Approvals
{
    public interface IApprovalEscalationService
    {
        Task ProcessAsync(CancellationToken ct = default);
    }
}
