using FVN_REGISTER.Contract.Dtos.EmailTemplates;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Shared.Services.Emails
{
    public interface IEmailQueueClientService
    {
        Task<ApiResponse<List<EmailQueueDto>>> GetAllAsync(CancellationToken ct = default);
        Task<ApiResponse<object>> RetryAsync(int id, CancellationToken ct = default);
        Task<ApiResponse<object>> RetryBatchAsync(List<int> ids, CancellationToken ct = default);
        Task<ApiResponse<object>> CancelAsync(int id, CancellationToken ct = default);
        Task<ApiResponse<object>> CancelBatchAsync(List<int> ids, CancellationToken ct = default);
        Task<ApiResponse<object>> TriggerProcessAsync(CancellationToken ct = default);
    }
}
