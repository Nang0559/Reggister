


using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Maps
{
    public static class AttachmentMapper
    {
        public static AttachmentDto ToDto(F03Attachment a) => new()
        {
            FileId = a.FileId,
            FileName = a.FileName ?? "Unknown",
            FilePath = a.FilePath ?? string.Empty,
            FileExtension = Path.GetExtension(a.FileName),
            FileSize = a.FileSize,
            UploadedAt = a.CreatedAt ?? DateTime.MinValue
        };

        public static List<AttachmentDto> ToDtoList(IEnumerable<F03Attachment>? attachments)
            => attachments?.Select(ToDto).ToList() ?? new List<AttachmentDto>();

        // Bổ sung Module + RequestId + fileSize — bắt buộc để đúng thiết kế polymorphic
        public static F03Attachment ToEntity(
            RequestModule module,
            int requestId,
            string fileName,
            string filePath,
            long fileSize,
            int uploadedBy) => new()
            {
                Module = module,
                RequestId = requestId,
                FileName = fileName,
                FilePath = filePath,
                FileExtension = Path.GetExtension(fileName),
                FileSize = fileSize,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = uploadedBy,
                IsActive = true
            };
    }
}
