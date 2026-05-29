using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class F03user
{
    public int IdUser { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Avatar { get; set; }

    public string? EmployeeCode { get; set; }

    public int PermissionCode { get; set; }

    public DateTime? LastLogin { get; set; }

    public bool? LockoutEnable { get; set; }

    public DateTime? LockoutEndDate { get; set; }

    public int? NumLoginFailed { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? LevelApprove { get; set; }

    public string? DeptCode { get; set; }

    public string? Cvcode { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<F03userFunction> F03userFunctions { get; set; } = new List<F03userFunction>();

    public virtual F03permission PermissionCodeNavigation { get; set; } = null!;
}
