namespace FVN_REGISTER.Contract.Requests.HrmSync;

public sealed class HrmUserRoleRuleRequest
{
    public string? DeptCode { get; set; }
    public string? PositionCode { get; set; }
    public int PermissionCode { get; set; }
    public int Priority { get; set; } = 100;
    public string? Note { get; set; }
    public bool IsActive { get; set; } = true;
}