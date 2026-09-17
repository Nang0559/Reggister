
using FVN_REGISTER.Contract.Dtos.Approvals;

namespace FVN_REGISTER.Contract.Interfaces
{
    public interface IHasAttachments
    {
        // Đảm bảo tên thuộc tính này khớp với các ViewModel của bạn
        int Id { get; set; }
        List<AttachmentDto> Attachments { get; set; }
    }
}
