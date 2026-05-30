using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Shared.Services.Leaves
{
    public interface IApproveClientService
    {
        Task<ApiResponse<List<PendingApprovalGroup>>> GetPendingAsync(
            CancellationToken ct = default);

        Task<ApiResponse<object>> ApproveAsync(
            List<int> ids, int level, string? comment,
            CancellationToken ct = default);

        Task<ApiResponse<object>> RejectAsync(
            List<int> ids, int level, string? comment,
            CancellationToken ct = default);
    }
}
