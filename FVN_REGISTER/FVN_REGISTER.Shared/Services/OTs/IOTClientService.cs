using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Contract.ViewModels.OT;

namespace FVN_REGISTER.Shared.Services.OT;

public interface IOTClientService
{
    Task<ApiResponse<OTRequestDto>> CreateAsync(CreateOTRequestModel model, CancellationToken ct = default);
    Task<ApiResponse<OTValidationResult>> CheckHoursAsync(DateOnly otDate, List<OTEmployeeModel> employees, CancellationToken ct = default);
    Task<ApiResponse<object>> ApproveAsync(List<int> ids, int level, string? comment, CancellationToken ct = default);
    Task<ApiResponse<object>> RejectAsync(List<int> ids, int level, string? comment, CancellationToken ct = default);
    Task<ApiResponse<object>> CancelAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<List<OTRequestDto>>> GetMyOTAsync(int? year, CancellationToken ct = default);
    Task<ApiResponse<List<OTRequestDto>>> GetPendingAsync(CancellationToken ct = default);
    Task<ApiResponse<OTRequestDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<List<EmployeeSelectDto>>> GetEmployeesByDeptAsync(
    string deptCode, CancellationToken ct = default);
}