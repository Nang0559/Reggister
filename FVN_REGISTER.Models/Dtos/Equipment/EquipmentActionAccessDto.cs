namespace FVN_REGISTER.Contract.Dtos.Equipment;

public sealed class EquipmentActionAccessDto
{
    public bool View { get; set; }
    public bool Create { get; set; }
    public bool Edit { get; set; }
    public bool Assign { get; set; }
    public bool Transfer { get; set; }
    public bool Return { get; set; }
    public bool Repair { get; set; }
    public bool Liquidate { get; set; }
    public bool Approve { get; set; }
    public bool Import { get; set; }
    public bool Export { get; set; }
    public bool QR { get; set; }
    public bool History { get; set; }
}
