using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class VEmployeeApprover
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
