using FVN_REGISTER.Contract.Interfaces.Emails;
using FVN_REGISTER.Contract.Interfaces.OT;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Services;
using Microsoft.Extensions.Options;
using FVN_REGISTER.Core.Logging;

namespace FVN_REGISTER.API.Services.OT
{
    public class OTNotificationService
     : BaseService<OTNotificationService>, IOTNotificationService
    {
        private readonly IEmailService _email;

        public OTNotificationService(
            IEmailService email,
            ILogger<OTNotificationService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _email = email;
        }

        public async Task SendApprovalRequestAsync(
            F03OTRequest request, string? toEmail, string stepName)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) return;

            Logger.LogDebugIf(Debug,
                "[OT-MAIL] Approval request → {Email} | Step: {Step}",
                toEmail, stepName);

            await _email.QueueEmail(toEmail, "OT_REQUEST_APPROVE", new
            {
                StepName = stepName,
                OTDate = request.OTDate.ToString("dd/MM/yyyy"),
                PlannedFrom = request.PlannedFrom.ToString("HH:mm"),
                PlannedTo = request.PlannedTo.ToString("HH:mm"),
                Hours = request.PlannedHours,
                Reason = request.OTReason,
                EmployeeCode = request.EmployeeCode,
                RequiresGM = request.RequiresGM ? "CÓ" : "KHÔNG",
                Url = $"https://yourdomain/ot/approve/{request.Id}"
            });
        }

        public async Task SendRejectedAsync(F03OTRequest request, string rejectedBy)
        {
            if (string.IsNullOrWhiteSpace(request.CreatedByEmail)) return;

            await _email.QueueEmail(request.CreatedByEmail, "OT_REQUEST_REJECTED", new
            {
                RejectedBy = rejectedBy,
                OTDate = request.OTDate.ToString("dd/MM/yyyy"),
                Hours = request.PlannedHours,
                Url = $"https://yourdomain/ot/detail/{request.Id}"
            });
        }

        public async Task SendApprovedAsync(F03OTRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CreatedByEmail)) return;

            await _email.QueueEmail(request.CreatedByEmail, "OT_REQUEST_APPROVED", new
            {
                OTDate = request.OTDate.ToString("dd/MM/yyyy"),
                Hours = request.PlannedHours,
                Url = $"https://yourdomain/ot/detail/{request.Id}"
            });
        }
    }
}
