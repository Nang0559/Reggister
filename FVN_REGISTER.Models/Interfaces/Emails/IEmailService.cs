using FVN_REGISTER.Contract.Models;


namespace FVN_REGISTER.Contract.Interfaces.Emails
{
    public interface IEmailService
    {
        Task QueueEmail(
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

        Task<List<EmailQueue>> GetFailedEmails(CancellationToken ct = default);

        Task CancelEmail(int id, CancellationToken ct = default);

        Task ResendEmail(int id, CancellationToken ct = default);
    }
}
