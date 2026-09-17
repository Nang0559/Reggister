


using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Dtos.OtReasons;
using FVN_REGISTER.Contract.Requests.OT;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Utils;

namespace FVN_REGISTER.Application.Interfaces.OT
{
    public interface IOTValidator
    {
        /// <summary>
        /// Validate giới hạn giờ OT (ngày/tháng/năm) cho 1 nhân viên khi sửa/thêm giờ thủ công
        /// (ví dụ UpdateEmployeeOTInfoAsync). Giới hạn tháng/năm tính theo (B): ưu tiên
        /// F03OTEmployee.ActualHours (đã đối chiếu qua OTAttendanceReconciliationService),
        /// fallback OTHours (đăng ký) cho các đơn chưa đối chiếu.
        /// </summary>
        /// <param name="excludeOTRequestId">Loại trừ chính đơn đang sửa khỏi tổng giờ đã dùng, tránh tự cộng dồn với chính nó.</param>
        Task<OTValidationResultDto> ValidateEmployeeHoursAsync(
            string employeeCode,
            DateTime otDate,
            decimal hours,
            string otTypeCode,
            CancellationToken ct = default,
            int? excludeOTRequestId = null);

        /// <summary>
        /// Validate toàn bộ đơn OT trước khi tạo mới — giới hạn giờ từng nhân viên (theo B),
        /// bắt buộc chọn lý do, và bắt buộc hierarchy duyệt phải đủ approver + không tự duyệt.
        /// </summary>
        Task<ServiceResult> ValidateCreateAsync(
            OTRequestUpsertDto model,
            UserIdentityDto user,
            CancellationToken ct = default);
    }
}
