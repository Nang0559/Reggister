namespace FVN_REGISTER.Contract.Dtos.Equipment;

public sealed class EquipmentApproverDto
{
    public string ApproverCode { get; set; } = string.Empty;
    public string ApproverName { get; set; } = string.Empty;
    public string ApproverEmail { get; set; } = string.Empty;
    public int Level { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string ApproveForDeptCode { get; set; } = string.Empty;
}
