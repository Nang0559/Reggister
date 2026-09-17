using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.ViewModels.Leaves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Shared.Services.Leaves
{
    public interface ILeaveHistorysClientService
    {
        Task<ApiResponse<List<LeaveRequestViewModel>>> GetHistoryAsync(
            int? year,
            string? status,
            CancellationToken ct = default);
    }
}
