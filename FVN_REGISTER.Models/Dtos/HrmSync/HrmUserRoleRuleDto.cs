namespace FVN_REGISTER.Contract.Dtos.HrmSync;

public sealed class HrmUserRoleRuleDto
{
    public int Id { get; set; }
    public bool IsActive { get; set; }
    public string? DeptCode { get; set; }
    public string? DeptName { get; set; }
    public string? PositionCode { get; set; }
    public string? PositionName { get; set; }
    public int PermissionCode { get; set; }
    public string? PermissionName { get; set; }
    public int Priority { get; set; }
    public string? Note { get; set; }
}