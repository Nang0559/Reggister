namespace FVN_REGISTER.Contract.Dtos.Notifications;

public sealed class TripDetailPayload
{
    public string? EmployeeName { get; set; }
    public string? DeptName { get; set; }
    public string TripCode { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string? CustomerOrPartner { get; set; }
    public string? TransportMethod { get; set; }
    public string? CompanionEmployeeCodes { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? Accommodation { get; set; }
    public string? Note { get; set; }
}