using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Contract.Dtos.Approvals;

public sealed class PendingApprovalItemDto
{
    public int RequestId { get; set; }

    public RequestModule Kind { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;

    public string DeptCode { get; set; } = string.Empty;
    public string DeptName { get; set; } = string.Empty;

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public decimal TotalUnits { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public int? TimeoutDays { get; set; }

    public List<ApprovalStepDto> ApprovalSteps { get; set; } = new();

    public bool CanApprove { get; set; }
}
