using FVN_REGISTER.Contract.Dtos.Usermanagers;
using FVN_REGISTER.Core.Entities.Security;


namespace FVN_REGISTER.Application.Maps
{
  
        public static class UserAccountMapper
        {
            public static UserAccountDto ToDto(
                F03User user, string? deptName, string? permissionName, List<int> functionIds) => new()
                {
                    IdUser = user.IdUser,
                    EmployeeCode = user.EmployeeCode,
                    FullName = user.FullName,
                    DeptCode = user.DeptCode,
                    DeptName = deptName,
                    PermissionCode = user.PermissionCode,
                    PermissionName = permissionName,
                    LevelApprove = user.LevelApprove,
                    Cvcode = user.Cvcode,
                    IsActive = user.IsActive ?? false,      // IsActive vẫn nullable — kế thừa từ BaseAuditEntity (bool?)
                    LockoutEnable = user.LockoutEnable,      // non-nullable — bỏ ?? false
                    LockoutEndDate = user.LockoutEndDate,
                    LastLogin = user.LastLogin,
                    NumLoginFailed = user.NumLoginFailed,    // non-nullable — bỏ ?? 0
                    Avatar = user.Avatar,
                    FunctionIds = functionIds
                };
        }
    
}
