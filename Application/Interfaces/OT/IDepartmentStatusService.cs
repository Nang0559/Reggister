

using FVN_REGISTER.Contract.Dtos.Depts;

namespace FVN_REGISTER.Application.Interfaces.OT
{
    public interface IDepartmentStatusService
    {
        /// <summary>
        /// Lấy trạng thái hiện diện của tất cả nhân viên trong một phòng ban
        /// cho ngày chỉ định (mặc định hôm nay)
        /// </summary>
        Task<DepartmentStatusDto> GetDeptStatusAsync(
            string deptCode,
            DateTime? date = null,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy tổng hợp toàn công ty — mỗi phòng một dòng summary
        /// </summary>
        Task<List<DepartmentStatusDto>> GetAllDeptStatusAsync(
            DateTime? date = null,
            CancellationToken ct = default);
    }
}
