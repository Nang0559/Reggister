using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Contract.ViewModels.OT;


namespace FVN_REGISTER.Contract.Interfaces.OT
{
    public interface IOTValidator
    {
        Task<ServiceResult> ValidateAsync(
            CreateOTRequestModel model,
            CurrentUser user,
            CancellationToken ct = default);
    }
}
