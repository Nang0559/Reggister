

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
        // Nhận vào int? thay vì string?
        public static bool IsSuperAdmin(this int? code)
            => code == UserPermissionCodes.SuperAdmin;

        public static bool IsAdmin(this int? code)
            => code == UserPermissionCodes.SuperAdmin || code == UserPermissionCodes.Admin;

        public static bool IsApprover(this int? code)
            => code == UserPermissionCodes.SuperAdmin ||
               code == UserPermissionCodes.Admin ||
               code == UserPermissionCodes.Approver;
    }
}
