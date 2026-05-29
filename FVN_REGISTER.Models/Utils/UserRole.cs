

namespace FVN_REGISTER.Contract.Utils
{
    [Flags]
    public enum UserRole
    {
        None = 0,
        SuperAdmin = 1,
        Admin = 2,
        Approver = 4,
        User = 8
    }
}
