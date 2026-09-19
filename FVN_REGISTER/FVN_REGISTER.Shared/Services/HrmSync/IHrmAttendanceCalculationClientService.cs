using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Contract.Responses;
namespace FVN_REGISTER.Shared.Services.HrmSync;
public interface IHrmAttendanceCalculationClientService
{
 Task<ApiResponse<HrmAttendanceCalculationResultDto>> CalculateAsync(HrmAttendanceCalculationRequestDto request,CancellationToken ct=default);
}
