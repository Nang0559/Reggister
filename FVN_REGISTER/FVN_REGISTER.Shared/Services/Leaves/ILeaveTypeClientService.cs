using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.ViewModels.Leaves;


namespace FVN_REGISTER.Shared.Services.Leaves
{
    public interface ILeaveTypeClientService
    {
        /// <summary>Lấy danh sách phẳng (toàn bộ hoặc filter).</summary>
        Task<ApiResponse<List<LeaveTypeFlatViewModel>>> GetListAsync(
            bool? tinhPhep = null,
            bool? isActive = null,
            CancellationToken ct = default);

        /// <summary>Lấy cây nhóm theo IsActive (client-side grouping).</summary>
        Task<ApiResponse<List<LeaveTypeGroupViewModel>>> GetTreeAsync(CancellationToken ct = default);

        Task<ApiResponse<object>> CreateAsync(LeaveTypeUpsertModel model, CancellationToken ct = default);
        Task<ApiResponse<object>> UpdateAsync(LeaveTypeUpsertModel model, CancellationToken ct = default);
        Task<ApiResponse<object>> ToggleAsync(int id, CancellationToken ct = default);
        Task<ApiResponse<object>> DeleteAsync(int id, CancellationToken ct = default);
    }
}
