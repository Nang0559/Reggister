using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.ViewModels;


namespace FVN_REGISTER.Contract.Interfaces.Users
{
    public interface IUserService
    {
        Task<List<VF03user>> GetAllAsync(CancellationToken ct = default);

        Task<F03user?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<ServiceResult> CreateAsync(
            UserAccountViewModel model,
            int currentUserId,
            CancellationToken ct = default);

        Task<ServiceResult> UpdateAsync(
            UserAccountViewModel model,
            int currentUserId,
            CancellationToken ct = default);

        Task<ServiceResult> DeleteAsync(
            int id,
            int currentUserId,
            CancellationToken ct = default);
    }
}
