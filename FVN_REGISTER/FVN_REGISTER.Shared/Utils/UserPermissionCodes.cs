

// File: FVN_REGISTER/FVN_REGISTER.Shared/Utils/UserPermissionCodes.cs
// THAY THẾ file cũ bằng file này — chỉ thêm method IsNormalUser

namespace FVN_REGISTER.Shared.Utils
{
    public static class UserPermissionCodes
    {
        public const int SuperAdmin = 1;
        public const int Admin = 2;
        public const int Approver = 4;
        public const int User = 5;
    }

    public static class UserRoleHelper
    {
        public static bool IsSuperAdmin(this int? code)
            => code == UserPermissionCodes.SuperAdmin;

        public static bool IsAdmin(this int? code)
            => code == UserPermissionCodes.SuperAdmin
            || code == UserPermissionCodes.Admin;

        public static bool IsApprover(this int? code)
            => code == UserPermissionCodes.SuperAdmin
            || code == UserPermissionCodes.Admin
            || code == UserPermissionCodes.Approver;

        /// <summary>
        /// Nhân viên thường: chỉ có quyền User (code = 5).
        /// Những role này sẽ thấy UserDashboard dạng widget, không có sidebar.
        /// </summary>
        public static bool IsNormalUser(this int? code)
            => code == UserPermissionCodes.User;
    }
}