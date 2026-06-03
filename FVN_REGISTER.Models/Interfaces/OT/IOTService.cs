using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Contract.ViewModels.OT;


namespace FVN_REGISTER.Contract.Interfaces.OT
{
    public interface IOTService
    {
        Task<ServiceResult<OTRequestDto>> CreateAsync(
            CreateOTRequestModel model,
            CurrentUser user,
            CancellationToken ct = default);

        Task<ServiceResult> ApproveAsync(
            List<int> ids, int level,
            CurrentUser user, string? comment,
            CancellationToken ct = default);

        Task<ServiceResult> RejectAsync(
            List<int> ids, int level,
            CurrentUser user, string? comment,
            CancellationToken ct = default);

        Task<ServiceResult> CancelAsync(
            int id, CurrentUser user,
            CancellationToken ct = default);

        Task<ServiceResult<OTRequestDto>> GetByIdAsync(
            int id, CancellationToken ct = default);

        Task<ServiceResult<List<OTRequestDto>>> GetByEmployeeAsync(
            string employeeCode, int? year,
            CancellationToken ct = default);

        Task<ServiceResult<List<OTRequestDto>>> GetPendingForApproverAsync(
            string approverEmail,
            CancellationToken ct = default);

        /// <summary>Kiểm tra rule 40h/tuần và 300h/năm</summary>
        Task<OTValidationResult> CheckOTHoursRuleAsync(
            DateOnly otDate,
            List<OTEmployeeModel> employees,
            CancellationToken ct = default);
        Task<ApiResponse<List<EmployeeSelectDto>>> GetEmployeesByDeptAsync(
       string deptCode, CancellationToken ct = default);
    }
}
