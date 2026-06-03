using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Interfaces.OT
{
    public interface IOTQueryService
    {
        Task<ServiceResult<List<VF03OTRequestSummary>>> GetPendingForApproverAsync(
            string approverEmail, CancellationToken ct = default);

        Task<ServiceResult<List<VF03OTRequestSummary>>> GetByEmployeeAsync(
            string employeeCode, int year, CancellationToken ct = default);

        Task<ServiceResult<List<OTApproverSelectDto>>> GetApproversAsync(
            string deptCode, int level, CancellationToken ct = default);

        Task<ServiceResult<OTSummaryDto>> GetSummaryAsync(
            string employeeCode, int year, int month, CancellationToken ct = default);
    }
}
