

namespace FVN_REGISTER.Core.Interfaces
{
    /// <summary>
    /// Đại diện 1 dòng request tối giản để hiển thị trong danh sách chờ duyệt.
    /// Mỗi domain (Leave, OT...) tự quyết định ApproverLevel hiển thị field gì.
    /// </summary>
    public interface IPendingRequestRow
    {
        int RequestId { get; }
    }
}
