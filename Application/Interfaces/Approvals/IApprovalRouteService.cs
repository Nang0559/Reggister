using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Interfaces.Approvals;

public interface IApprovalRouteService
{
    Task<ServiceResult<ApprovalRoutePreviewDto>> GetPreviewAsync(
        RequestModule requestType,
        string employeeCode,
        string deptCode,
        string positionCode,
        CancellationToken ct = default);
}
