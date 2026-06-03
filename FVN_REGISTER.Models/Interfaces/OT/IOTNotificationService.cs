using Azure.Core;
using FVN_REGISTER.Contract.Models;


namespace FVN_REGISTER.Contract.Interfaces.OT
{
    public interface IOTNotificationService
    {
        Task SendApprovalRequestAsync(F03OTRequest request, string? toEmail, string stepName);
        Task SendRejectedAsync(F03OTRequest request, string rejectedBy);
        Task SendApprovedAsync(F03OTRequest request);
    }
}
