using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class VF03user
{
    public int IdUser { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Avatar { get; set; }

    public string? EmployeeCode { get; set; }

    public string? EmployeeName { get; set; }

    public string? DeptCode { get; set; }

    public string? DeptName { get; set; }

    public int? GenderCode { get; set; }

    public string? GenderName { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? EmailAddress { get; set; }

    public string? PhoneNumber { get; set; }

    public int PermissionCode { get; set; }

    public string? PermissionName { get; set; }

    public DateTime? LastLogin { get; set; }

    public bool? LockoutEnable { get; set; }

    public DateTime? LockoutEndDate { get; set; }

    public int? NumLoginFailed { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string? Cvcode { get; set; }

    public int? LevelApprove { get; set; }
}
