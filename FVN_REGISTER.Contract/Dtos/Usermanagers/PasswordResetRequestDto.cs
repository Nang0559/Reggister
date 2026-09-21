namespace FVN_REGISTER.Contract.Dtos.Usermanagers;

public sealed class PasswordResetRequestDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? DeptCode { get; set; }
    public string? RequestNote { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int? ProcessedBy { get; set; }
    public string? ProcessorName { get; set; }
    public string? ResultNote { get; set; }
}
