

using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.ViewModels;

namespace FVN_REGISTER.Contract.Interfaces.Leaves
{
    public interface ILeaveNotificationService
    {
        // 1. Khi có đơn mới: Gửi cho người duyệt cấp tiếp theo
        Task SendApprovalRequestAsync(string approverEmail, string approverName, F03leaveDay leave, string employeeName, CancellationToken ct);

        // 2. Khi trạng thái thay đổi: Gửi cho chủ đơn (Approved/Rejected)
        Task SendStatusChangedNotificationAsync(string targetEmail, string targetName, string status, string employeeCode, CancellationToken ct);

        // 3. Phương thức quét tự động (Batch Job): Gửi nhắc nhở các đơn chưa duyệt
        Task SendApprovalNotificationAsync(CancellationToken ct);
    }
}
