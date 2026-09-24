using FVN_REGISTER.Contract.Dtos.EmailTemplates;
using FVN_REGISTER.Core.Utils;

namespace FVN_REGISTER.Application.Interfaces.Emails;

public interface IEmailService
{
    Task<EmailQueueResult> QueueEmail(string to, string templateCode, object data, CancellationToken ct = default);
    Task ProcessQueue(CancellationToken ct = default);
    Task SendEmail(string to, string subject, string body, CancellationToken ct = default);
    string RenderTemplate(string template, string jsonPayload);
    Task<List<EmailQueueDto>> GetQueueAsync(int take = 500, CancellationToken ct = default);
    Task RetryEmailBatchAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
    Task CancelEmailBatchAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
    Task CancelEmail(int id, CancellationToken ct = default);
    Task ApproveEmail(int id, int userId, CancellationToken ct = default);
    Task ResendEmail(int id, CancellationToken ct = default);
}
