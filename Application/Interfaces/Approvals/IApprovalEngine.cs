using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Interfaces.Approvals
{
    /// <summary>
    /// Module-specific approval engine contract.
    /// Infrastructure owns the EF/transaction implementation.
    /// </summary>
    public interface IApprovalEngine<TSubject> where TSubject : IApprovalSubject
    {
        RequestModule Module { get; }

        Task InitializeStepsAsync(
            int requestId,
            ApprovalSnapshotDto snapshot,
            CancellationToken ct);

        Task<ApprovalActionResult> ProcessDecisionAsync(
            ApprovalActionDto action,
            CancellationToken ct);

        Task<List<PendingApprovalItemDto>> GetPendingForApproverAsync(
            string approverEmail,
            CancellationToken ct);

        Task<List<ApprovalStepCalculatedDto>> GetStepsAsync(
            int requestId,
            CancellationToken ct);

        Task<ApprovalStepDto?> GetNextStepAsync(
            int requestId,
            CancellationToken ct);
    }

    /// <summary>
    /// Cross-module dispatcher/resolver. It is the Application boundary used by
    /// API/Dashboard code and must not expose Infrastructure or EF types.
    /// </summary>
    public interface IApprovalEngineResolver
    {
        Task<ApprovalActionResult> ProcessDecisionAsync(
            RequestModule module,
            ApprovalActionDto action,
            CancellationToken ct);

        Task<List<PendingApprovalItemDto>> GetPendingForApproverAsync(
            RequestModule module,
            string approverEmail,
            CancellationToken ct);
    }
}
