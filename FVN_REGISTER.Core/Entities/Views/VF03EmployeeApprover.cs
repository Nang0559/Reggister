using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Core.Entities.Views;

public partial class VF03EmployeeApprover
{
    public int Id { get; set; }

    public string DeptCode { get; set; } = null!;

    public string DeptName { get; set; } = null!;

    public int ApproveLevel { get; set; }

    public string? RoleName { get; set; }

    public string ApproverName { get; set; } = null!;

    public string ApproverEmail { get; set; } = null!;

    public string? ApproveLevelCode { get; set; }

    public bool IsActive { get; set; }
}
