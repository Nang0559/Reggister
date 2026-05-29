

namespace FVN_REGISTER.Contract.Utils
{
    public static class StaticString
    {
        // Tên Form/Module
        public const string FormLeaveDays = "LeaveDays";

        

        // Các vai trò duyệt (Dùng cho Notification hoặc UI)
        public const string RoleApprover = "Approver";
        public const string RoleAdmin = "Admin";
        public const string RoleUser = "User";

        // Tùy chọn nghỉ nửa ngày (Half Day Options)
        // Khớp với field HalfDayOption trong database
        public const string Morning = "Morning";           // Nghỉ sáng
        public const string Afternoon = "Afternoon";       // Nghỉ chiều
    }
}
