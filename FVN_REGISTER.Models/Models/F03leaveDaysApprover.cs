using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class F03leaveDaysApprover
{
    public int Id { get; set; }

    public string DeptCode { get; set; } = null!;

    public int ApproveLevel { get; set; }

    public string ApproveLevelName { get; set; } = null!;

    public string ApproveLevelEmail { get; set; } = null!;

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; }

    public string? ApproveLevel1Name { get; set; }

    public string? ApproveLevel1Email { get; set; }

    public string? ApproveLevel2Name { get; set; }

    public string? ApproveLevel2Email { get; set; }

    public string? ApproveLevel3Name { get; set; }

    public string? ApproveLevel3Email { get; set; }

    public string? ApproveLevelCode { get; set; }
}
