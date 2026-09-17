using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Requests.Leaves;
using FVN_REGISTER.Core.Utils;


namespace FVN_REGISTER.Application.Interfaces.Common
{
    /// <summary>
    /// Contract chung cho mọi domain có luồng Tạo đơn → Duyệt/Từ chối → Hủy
    /// (Leave, Overtime, Trip...). Domain implement qua BaseRequestCommandService.
    /// </summary>
    public interface IRequestCommandService<TCreateModel>
    {
        Task<ServiceResult<int>> CreateAsync(
            TCreateModel model, UserIdentityDto user, CancellationToken ct = default);

        /// <summary>
        /// Duyệt hàng loạt (bulk) các đơn CÙNG 1 level. Đơn nào không đủ điều kiện
        /// (đã xử lý, sai approver...) sẽ bị bỏ qua chứ không làm fail cả batch;
        /// Message trả về sẽ nêu rõ "Đã xử lý X/Y đơn".
        /// </summary>
        Task<ServiceResult> ApproveAsync(
            List<int> ids, int level, UserIdentityDto user, string? comment, CancellationToken ct = default);

        /// <summary>
        /// Từ chối hàng loạt (bulk) các đơn CÙNG 1 level. Comment bắt buộc.
        /// Cùng cơ chế partial-success như ApproveAsync.
        /// </summary>
        Task<ServiceResult> RejectAsync(
            List<int> ids, int level, UserIdentityDto user, string comment, CancellationToken ct = default);

        Task<ServiceResult> CancelAsync(
            int requestId, string reason, UserIdentityDto user, CancellationToken ct = default);
        Task<ServiceResult> AttachFilesAsync(AttachFilesCommandDto command, UserIdentityDto user, CancellationToken ct = default);
    }
}
