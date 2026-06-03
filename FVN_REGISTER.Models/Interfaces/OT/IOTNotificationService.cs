using Azure.Core;
using FVN_REGISTER.Contract.Models;


namespace FVN_REGISTER.Contract.Interfaces.OT
{
    public interface IOTNotificationService
    {
        Task SendApprovalRequestAsync(
            string approverEmail,
            F03OTRequest otRequest,
            string requesterName,
            CancellationToken ct = default);

        Task SendStatusChangedAsync(
            F03OTRequest otRequest,
            string newStatus,
            CancellationToken ct = default);
    }
}
