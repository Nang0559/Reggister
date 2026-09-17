


using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Requests.OT;
using FVN_REGISTER.Contract.Utils;


namespace FVN_REGISTER.Application.Interfaces.OT
{
    public interface IOTService : IRequestCommandService<OTRequestUpsertDto>
    {
        // CreateAsync, ApproveAsync, RejectAsync, CancelAsync đã có từ IRequestCommandService.

        /// <summary>Nhân viên tự thêm mình vào đơn OT đang Draft/Pending của phòng.</summary>
        Task<ServiceResult> JoinAsync(
            int otRequestId, UserIdentityDto user, CancellationToken ct = default);

        /// <summary>Xóa nhân viên khỏi đơn OT (soft delete).</summary>
        Task<ServiceResult> RemoveEmployeeAsync(
            int otRequestId, string employeeCode, UserIdentityDto user, CancellationToken ct = default);

        /// <summary>Cập nhật giờ OT thực tế + lý do cho từng nhân viên trong đơn.</summary>
        Task<ServiceResult> UpdateEmployeeOTInfoAsync(
            int otRequestId, List<OTEmployeeDto> updates, UserIdentityDto user, CancellationToken ct = default);
       
        Task<ServiceResult> AddEmployeesAsync(int otRequestId, List<OTEmployeeDto> newEmployees, UserIdentityDto user, CancellationToken ct = default);
    }
}
