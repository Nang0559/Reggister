

namespace FVN_REGISTER.Core.Enums
{
    public enum ValidationStatus
    {
        Pending,   // Chưa kiểm tra
        Valid,     // Hợp lệ
        Warning,   // Có cảnh báo (nhưng vẫn có thể gửi đơn)
        Invalid    // Lỗi (không được phép gửi đơn)
    }
}
