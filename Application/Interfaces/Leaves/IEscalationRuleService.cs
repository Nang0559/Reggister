

using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Interfaces.Leaves
{
    public interface IEscalationRuleService
    {
        /// <summary>
        /// Lấy toàn bộ quy tắc leo thang cho một bước duyệt,
        /// bao gồm thời gian cảnh báo, leo thang và mốc deadline.
        /// </summary>
        Task<EscalationRuleDto?> GetRuleAsync(
            RequestModule requestType,   // string → RequestModule
            int level,
            string deptCode,
            CancellationToken ct = default);

        /// <summary>
        /// Tính toán mốc thời gian deadline cụ thể trong ngày dựa trên giờ quy định.
        /// </summary>
        /// <param name="registerDate">Ngày viết đơn</param>
        /// <param name="deadlineHour">Giờ giới hạn (ví dụ: 14 hoặc 16 từ cột DeadlineHour trong DB)</param>
        /// <returns>Mốc thời gian DateTime với giờ đã chỉ định</returns>
        DateTime GetDeadline(DateTime registerDate, int deadlineHour);
    }
}
