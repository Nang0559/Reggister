namespace FVN_REGISTER.Core.Constants
{
    
    public enum UserRole
    {
        None = 0,
        SuperAdmin = 1,
        Admin = 2,
        Editor = 3,
        [Obsolete("Legacy role. Approval is resolved by F03ApprovalPolicies + ApprovalRouteService.")]
        Approver = 4,
        User = 5,
        Guest = 6,
        HR = 7,
        IT = 8
    }
}
