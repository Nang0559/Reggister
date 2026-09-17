using FVN_REGISTER.Contract.Dtos.EmailTemplates;
using FVN_REGISTER.Core.Entities.Leaves;
using FVN_REGISTER.Core.Utils;

namespace FVN_REGISTER.Application.Interfaces.Emails
{
    public interface IEmailService
    {
        Task<EmailQueueResult> QueueEmail(
            string to,
            string templateCode,
            object data,
            CancellationToken ct = default);

        Task ProcessQueue(CancellationToken ct = default);

        Task SendEmail(
            string to,
            string subject,
            string body,
            CancellationToken ct = default);

        string RenderTemplate(string template, string jsonPayload);

        Task RetryFailedEmails(CancellationToken ct = default);

        Task<int> GetPendingCount(CancellationToken ct = default);

        Task<int> GetFailedCount(CancellationToken ct = default);

        Task<List<EmailQueueDto>> GetFailedEmails(CancellationToken ct = default);

        Task<List<EmailQueueDto>> GetQueueAsync(
            int take = 500,
            CancellationToken ct = default);

        Task RetryEmailBatchAsync(
            IReadOnlyCollection<int> ids,
            CancellationToken ct = default);

        Task CancelEmailBatchAsync(
            IReadOnlyCollection<int> ids,
            CancellationToken ct = default);

        Task CancelEmail(int id, CancellationToken ct = default);

        Task ResendEmail(int id, CancellationToken ct = default);

        Task SendApprovalRequestAsync(
            string toEmail,
            string recipientRole,
            F03LeaveDay leave,
            string employeeName,
            CancellationToken ct = default);
    }
}
