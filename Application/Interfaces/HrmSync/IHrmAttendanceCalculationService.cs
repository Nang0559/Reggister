using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Contract.Utils;
namespace FVN_REGISTER.Application.Interfaces.HrmSync;
public interface IHrmAttendanceCalculationService
{
 Task<ServiceResult<HrmAttendanceCalculationResultDto>> CalculateAsync(HrmAttendanceCalculationRequestDto request,string? triggeredBy=null,CancellationToken ct=default);
}
