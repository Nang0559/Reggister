using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Application.Interfaces.Infrastructure
{
    public interface IApprovalSnapshotService
    {
        // Chụp lại quy trình hiện tại vào DB (Snapshot)
        Task CreateSnapshotAsync(IApprovalSubject subject, CancellationToken ct);

        // Lấy Snapshot đã đóng băng
        Task<ApprovalSnapshotDto> GetSnapshotAsync(int requestId, RequestModule module, CancellationToken ct);
    }
}
