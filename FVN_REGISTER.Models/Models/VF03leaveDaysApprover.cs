using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class VF03leaveDaysApprover
{
    public int Id { get; set; }

    public string DeptCode { get; set; } = null!;

    public string DeptName { get; set; } = null!;

    public int ApproveLevel { get; set; }

    public string? LevelName { get; set; }

    public string ApproveLevelName { get; set; } = null!;

    public string ApproveLevelEmail { get; set; } = null!;

    public bool IsActive { get; set; }

    public string? ApproveLevelCode { get; set; }
}
