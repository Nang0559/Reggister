using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.ViewModels;


namespace FVN_REGISTER.Contract.Interfaces.Leaves
{
    public interface ILeaveService
    {
        // ==========================================
        // COMMANDS (Ghi dữ liệu)
        // ==========================================

        // Thay UserSessionDto bằng CurrentUser để khớp với BaseController
        Task<ServiceResult> CreateLeaveAsync(CreateLeaveRequestModel model, CurrentUser user, CancellationToken ct = default);

        Task<ServiceResult> CancelAsync(int leaveId, string reason, CurrentUser user, CancellationToken ct = default);

        Task<ServiceResult> ApproveAsync(List<int> leaveIds, int level, CurrentUser user, string comment, CancellationToken ct = default);

        Task<ServiceResult> RejectAsync(List<int> leaveIds, int level, CurrentUser user, string comment, CancellationToken ct = default);

        // ==========================================
        // QUERIES (Lấy dữ liệu) - Đã bọc ServiceResult
        // ==========================================

        // Sửa lỗi bạn đang gặp ở đây:
        Task<ServiceResult<LeaveBalanceViewModel>> GetLeaveBalanceAsync(string employeeCode, int year, CancellationToken ct = default);

        Task<ServiceResult<LeaveDaysViewModel>> GetDetailsAsync(int leaveId, CancellationToken ct = default);

        // Các queries cho Dashboard cũng nên bọc lại để HandleResult hoạt động được
        Task<ServiceResult<LeaveBalanceDto>> GetPersonalBalanceAsync(string employeeCode, int year, CancellationToken ct = default);

        Task<ServiceResult<List<RecentLeaveRequestDto>>> GetRecentRequestsAsync(string employeeCode, int limit, CancellationToken ct = default);
    }
}
