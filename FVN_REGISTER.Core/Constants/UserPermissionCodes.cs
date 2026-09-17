// File: FVN_REGISTER/FVN_REGISTER.Shared/Utils/UserPermissionCodes.cs
namespace FVN_REGISTER.Core.Constants
{
    public static class UserPermissionCodes
    {
        public const int SuperAdmin = 1;
        public const int Admin = 2;
        public const int Editor = 3;
        public const int Approver = 4;
        public const int User = 5;
        public const int Guest = 6;
    }

    public static class UserFunctionCodes
    {
        // Reserved application-module function. Admin assigns it per user.
        public const int EquipmentModule = 900;
    }

    public static class UserRoleHelper
    {
        public static bool IsSuperAdmin(this int? code) => code == UserPermissionCodes.SuperAdmin;
        public static bool IsAdmin(this int? code) => code == UserPermissionCodes.SuperAdmin || code == UserPermissionCodes.Admin;
        public static bool IsEditor(this int? code) => code == UserPermissionCodes.SuperAdmin || code == UserPermissionCodes.Admin || code == UserPermissionCodes.Editor;
        public static bool IsApprover(this int? code) => code == UserPermissionCodes.SuperAdmin || code == UserPermissionCodes.Admin || code == UserPermissionCodes.Editor || code == UserPermissionCodes.Approver;
        public static bool IsUser(this int? code) => code == UserPermissionCodes.User;
        public static bool IsGuest(this int? code) => code == UserPermissionCodes.Guest;
        public static bool IsNormalUser(this int? code) => code == UserPermissionCodes.User || code == UserPermissionCodes.Guest;
        public static bool CanRequestLeave(this int? code) => code == UserPermissionCodes.Approver || code == UserPermissionCodes.User;
        public static bool CanViewReport(this int? code) => code >= 1 && code <= 4;
        public static bool CanManageData(this int? code) => code == UserPermissionCodes.SuperAdmin || code == UserPermissionCodes.Admin || code == UserPermissionCodes.Editor;
        public static bool IsSuperAdmin(this int code) => ((int?)code).IsSuperAdmin();
        public static bool IsAdmin(this int code) => ((int?)code).IsAdmin();
        public static bool IsEditor(this int code) => ((int?)code).IsEditor();
        public static bool IsApprover(this int code) => ((int?)code).IsApprover();
        public static bool IsUser(this int code) => ((int?)code).IsUser();
        public static bool IsGuest(this int code) => ((int?)code).IsGuest();
        public static bool IsNormalUser(this int code) => ((int?)code).IsNormalUser();
        public static bool CanRequestLeave(this int code) => ((int?)code).CanRequestLeave();
        public static bool CanViewReport(this int code) => ((int?)code).CanViewReport();
        public static bool CanManageData(this int code) => ((int?)code).CanManageData();
    }
}