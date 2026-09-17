using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Interfaces;


namespace FVN_REGISTER.Contract.Requests.Leaves
{
    // Requests/Leaves/AttachFilesCommandDto.cs — DTO riêng cho việc đính kèm, dùng chung mọi module
    public class AttachFilesCommandDto : IHasAttachments
    {
        public int Id { get; set; } // = RequestId của đơn đã tạo
        public List<AttachmentDto> Attachments { get; set; } = new();
    }
}
