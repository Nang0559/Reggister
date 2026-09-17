



using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Interfaces.Approvals
{

    public interface IApprovalEngine
    {
        public interface IApprovalEngineResolver
        {
            Task<ApprovalActionResult> ProcessDecisionAsync(RequestModule module, ApprovalActionDto action, CancellationToken ct);
            Task<List<PendingApprovalItemDto>> GetPendingForApproverAsync(RequestModule module, string approverEmail, CancellationToken ct);
        }
        RequestModule Module { get; }
        Task<ApprovalActionResult> ProcessDecisionAsync(ApprovalActionDto action, CancellationToken ct);
        Task<List<PendingApprovalItemDto>> GetPendingForApproverAsync(string approverEmail, CancellationToken ct);
    }
    public interface IApprovalEngineResolver
    {
        Task<ApprovalActionResult> ProcessDecisionAsync(RequestModule module, ApprovalActionDto action, CancellationToken ct);
        Task<List<PendingApprovalItemDto>> GetPendingForApproverAsync(RequestModule module, string approverEmail, CancellationToken ct);
    }
    public interface IApprovalEngine<TSubject> where TSubject : IApprovalSubject
    {
        // Sử dụng ApprovalSnapshotDto thay cho Hierarchy cũ
        Task InitializeStepsAsync(int requestId, ApprovalSnapshotDto snapshot, CancellationToken ct);

        // Sử dụng ApprovalActionDto thay cho ApprovalDecision
        Task<ApprovalActionResult> ProcessDecisionAsync(ApprovalActionDto action, CancellationToken ct);

        // Trả về PendingApprovalItemDto (DTO chuẩn cho danh sách chờ duyệt của bạn)
        Task<List<PendingApprovalItemDto>> GetPendingForApproverAsync(string approverEmail, CancellationToken ct);

        // Trả về danh sách các bước đã tính toán
        Task<List<ApprovalStepCalculatedDto>> GetStepsAsync(int requestId, CancellationToken ct);

        // Trả về DTO đại diện cho bước tiếp theo
        Task<ApprovalStepDto?> GetNextStepAsync(int requestId, CancellationToken ct);
    }
}
