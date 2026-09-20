namespace FVN_REGISTER.Contract.Dtos.OT;

public sealed class OTLimitPreviewDto
{
    public DateTime OTDate { get; set; }
    public decimal RequestedHours { get; set; }
    public List<OTLimitEmployeePreviewDto> Employees { get; set; } = new();
    public OTLimitScopePreviewDto? Department { get; set; }
    public OTLimitScopePreviewDto? Block { get; set; }
    public bool IsValid => Errors.Count == 0 && !Employees.Any(x => x.IsExceeded)
        && (Department == null || !Department.IsExceeded)
        && (Block == null || !Block.IsExceeded);
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
