using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Contract.Dtos.Approvals;

public sealed class ApprovalRoutePreviewDto
{
    public RequestModule RequestType { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = string.Empty;
    public string PositionCode { get; set; } = string.Empty;
    public List<ApprovalRouteLevelDto> Levels { get; set; } = new();
}
