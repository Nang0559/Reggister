using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Interfaces;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Interfaces.Common
{
    public interface IAttachmentService
    {
        /// <summary>
        /// Nạp Attachments vào từng item (batch — 1 query duy nhất cho N items,
        /// không N+1 query). Dùng ở BƯỚC 4 (Đọc lại) khi build LeaveRequestDto/OtRequestDto.
        /// </summary>
        Task EnrichAsync<T>(IEnumerable<T> items, RequestModule requestType, CancellationToken ct)
            where T : IHasAttachments;

        Task<List<AttachmentDto>> GetAttachmentsAsync(int requestId, RequestModule requestType, CancellationToken ct);
    }
}
