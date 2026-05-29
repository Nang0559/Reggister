using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.ViewModels;


namespace FVN_REGISTER.Contract.Interfaces.Leaves
{
    public interface ILeaveValidator
    {
        // Cập nhật tham số từ string sang UserSessionDto để đồng bộ với Service
        Task<ServiceResult> ValidateAsync(CreateLeaveRequestModel model, UserSessionDto user,CancellationToken ct);
    }
}
