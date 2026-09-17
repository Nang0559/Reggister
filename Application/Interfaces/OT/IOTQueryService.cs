using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Requests.OT;


namespace FVN_REGISTER.Application.Interfaces.OT
{
    public interface IOTQueryService
    : IRequestQueryService<OTSummaryDto, OTBalanceDto, OTRequestDto>
    {
        Task<OTCombinedDataDto> GetCombinedDataAsync(
            string employeeCode, string deptCode, int year, int month, CancellationToken ct = default);

        Task<OTValidationResultDto> ValidateHoursAsync(OTRequestUpsertDto model, CancellationToken ct = default);

        Task<OTBalanceDto> GetBalanceAsync(string employeeCode, int year, int month, CancellationToken ct = default);
        // ✅ giữ nguyên — khác mục đích thật với GetSimpleBalanceAsync (theo bạn xác nhận điểm 1):
        // GetBalanceAsync: cho FORM/VALIDATE, cần theo THÁNG cụ thể user đang nhập
        // GetSimpleBalanceAsync: cho DASHBOARD, mặc định theo năm/tháng hiện tại

        Task<List<OTEmployeeDto>> GetDeptEmployeesAsync(string deptCode, CancellationToken ct = default);

        Task<string> GetEmployeeDeptCodeAsync(string employeeCode, CancellationToken ct = default);

        Task<List<OTBalanceDto>> GetDeptNearLimitAsync(
         string deptCode, int year, int month, CancellationToken ct = default);
    }
}