using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;
namespace FVN_REGISTER.Shared.Services.HrmSync;
public sealed class HrmAttendanceCalculationClientService:IHrmAttendanceCalculationClientService
{
 private readonly IHttpClientWithAuth _http;
 public HrmAttendanceCalculationClientService(IHttpClientWithAuth http)=>_http=http;
 public Task<ApiResponse<HrmAttendanceCalculationResultDto>> CalculateAsync(HrmAttendanceCalculationRequestDto request,CancellationToken ct=default)
  => _http.PostAsync<HrmAttendanceCalculationResultDto>("api/hrm-attendance-calculation/calculate",request,ct);
 public Task<ApiResponse<byte[]>> ExportAttendanceAsync(Guid batchId,CancellationToken ct=default)
  => _http.GetFileAsync($"api/hrm-attendance-calculation/export/attendance/{batchId}",ct);
 public Task<ApiResponse<byte[]>> ExportOtAsync(Guid batchId,CancellationToken ct=default)
  => _http.GetFileAsync($"api/hrm-attendance-calculation/export/ot/{batchId}",ct);
}
