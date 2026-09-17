using FVN_REGISTER.Contract.Dtos.Positions;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Utils;


namespace FVN_REGISTER.Application.Interfaces.Companies
{
    public interface IPositionManagementService
    {
        Task<ServiceResult<List<PositionDto>>> GetAllAsync(CancellationToken ct = default);

        Task<ServiceResult<List<PositionDto>>> GetFilteredAsync(
            bool? isApprove, bool? isActive, CancellationToken ct = default);

        Task<ServiceResult<PositionDto>> GetByIdAsync(string PositionCode, CancellationToken ct = default);

        Task<ServiceResult> CreateAsync(
            PositionUpsertDto model, int currentUserId, CancellationToken ct = default);

        Task<ServiceResult> UpdateAsync(
            PositionUpsertDto model, int currentUserId, CancellationToken ct = default);

        Task<ServiceResult> ToggleActiveAsync(
            string PositionCode, int currentUserId, CancellationToken ct = default);

        /// <summary>Chặn xóa nếu còn nhân viên đang gắn PositionId này.</summary>
        Task<ServiceResult> DeleteAsync(string PositionCode, CancellationToken ct = default);
    }
}
