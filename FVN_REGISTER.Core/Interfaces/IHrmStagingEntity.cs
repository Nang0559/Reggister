using FVN_REGISTER.Core.Entities;
using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Core.Interfaces
{
    /// <summary>
    /// Dành riêng cho các bảng Staging phục vụ ĐỒNG BỘ THỰC THỂ từ HRM (Employee, Department...) —
    /// khác với IStagingData thuần (dùng cho import nghiệp vụ như F03StagingLeave/OT/Trip).
    /// Thêm EntityKey + Action vì đồng bộ thực thể cần biết "đổi gì" (Insert/Update/Delete),
    /// còn import nghiệp vụ chỉ cần "đẩy dữ liệu vào, xử lý xong thì đánh dấu".
    /// </summary>
    public interface IHrmStagingEntity : IStagingData
    {
        string EntityKey { get; }
        HrmChangeAction Action { get; set; }
    }
}
