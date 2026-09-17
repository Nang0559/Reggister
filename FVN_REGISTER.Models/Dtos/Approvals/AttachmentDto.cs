

namespace FVN_REGISTER.Contract.Dtos.Approvals
{
    public class AttachmentDto
    {
        public int FileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;

        // Bổ sung để UI xử lý tốt hơn
        public string? FileExtension { get; set; } // Ví dụ: ".pdf", ".jpg"
        public long FileSize { get; set; }         // Để cảnh báo file quá lớn
        public DateTime UploadedAt { get; set; }
    }
}
