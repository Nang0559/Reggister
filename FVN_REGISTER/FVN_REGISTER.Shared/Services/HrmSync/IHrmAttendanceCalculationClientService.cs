using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Contract.Responses;
namespace FVN_REGISTER.Shared.Services.HrmSync;
public interface IHrmAttendanceCalculationClientService
{
 Task<ApiResponse<HrmAttendanceCalculationResultDto>> CalculateAsync(HrmAttendanceCalculationRequestDto request,CancellationToken ct=default);
 Task<ApiResponse<byte[]>> ExportAttendanceAsync(Guid batchId,CancellationToken ct=default);
 Task<ApiResponse<byte[]>> ExportOtAsync(Guid batchId,CancellationToken ct=default);
}
