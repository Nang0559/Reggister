
using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Responses;


namespace FVN_REGISTER.Application.Interfaces.Orchestrators
{
    public interface IApprovalWorkflowOrchestrator<TSubject> where TSubject : IApprovalSubject
    {
        /// <summary>
        /// Build snapshot quy trình duyệt (dựa trên ApprovalBuildContext) và khởi tạo bước duyệt cho request vừa tạo.
        /// Gọi ngay sau khi entity gốc (F03LeaveDay/F03OTRequest) đã được lưu và có Id.
        /// </summary>
        Task InitApprovalAsync(int requestId, ApprovalBuildContext ctx, CancellationToken ct);

        Task<ApprovalActionResult> ApproveAsync(ApprovalActionDto action, CancellationToken ct);

        Task<ApprovalActionResult> RejectAsync(ApprovalActionDto action, CancellationToken ct);

        Task<List<PendingApprovalItemDto>> GetPendingForApproverAsync(string approverEmail, CancellationToken ct);

        Task<List<ApprovalStepDto>> GetStepsAsync(int requestId, CancellationToken ct);
    }
}
