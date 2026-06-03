

using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.ViewModels;

namespace FVN_REGISTER.Contract.Interfaces.Leaves
{
    public interface ILeaveNotificationService
    {
        Task NotifyNewLeaveRequestAsync(
                string approverEmail,
                int leaveId,
                string employeeName,
                CancellationToken cancellationToken);
        Task NotifyStatusChangedAsync(
        int requesterUserId, string targetEmail, string targetName,
        string status, string employeeCode, int leaveId, CancellationToken ct = default);
       
    }
}
