

using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Requests.Leaves;
using FVN_REGISTER.Contract.Utils;


namespace FVN_REGISTER.Application.Interfaces.Leaves
{
    public interface ILeaveValidator
    {
      
            Task<ServiceResult> ValidateAsync(
                LeaveRequestUpsertDto model,
                UserIdentityDto user,
                CancellationToken ct = default);
    }
    
}
