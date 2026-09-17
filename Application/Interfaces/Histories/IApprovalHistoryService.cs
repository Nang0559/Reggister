


using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Histories;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Interfaces.Histories
{
    public interface IApprovalHistoryService
    {
        // Ghi lại hành động duyệt/từ chối của user (batch theo RequestIds)
        Task RecordActionAsync(ApprovalActionDto action, CancellationToken ct);

        // Ghi lại trạng thái khởi tạo đơn ban đầu — dòng "Đã nộp đơn" trong timeline
        Task LogInitialStatusAsync(int requestId, RequestModule module, CancellationToken ct);

        // Lấy RAW audit log của 1 đơn — dùng làm input cho ApprovalStepMapper,
        // KHÔNG phải DTO hiển thị trực tiếp cho UI (đó là HistoryItemDto.ApprovalSteps)
        Task<List<F03ApprovalHistory>> GetHistoryAsync(int requestId, RequestModule module, CancellationToken ct);
    }
}
