using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Core.Entities.Trips;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Models.Subjects;

public sealed class TripRequestSubject : IApprovalSubject
{
    public int RequestId { get; set; }
    public RequestModule Module => RequestModule.Trip;
    public string EmployeeCode { get; set; } = string.Empty;
    public string? EmployeeName { get; set; }
    public string? DeptCode { get; set; }
    public string? PositionCode { get; set; }
    public ApprovalStatus OverallStatus { get; set; }
    public string TripCode { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string? CustomerOrPartner { get; set; }
    public string? TransportMethod { get; set; }
    public decimal? EstimatedCost { get; set; }

    public static TripRequestSubject From(
        F03TripRequest x,
        string? employeeName = null,
        string? positionCode = null) => new()
    {
        RequestId = x.Id,
        EmployeeCode = x.EmployeeCode,
        EmployeeName = employeeName,
        DeptCode = x.DeptCode,
        PositionCode = positionCode,
        OverallStatus = x.RequestStatus,
        TripCode = x.TripCode,
        StartDate = x.StartDate,
        EndDate = x.EndDate,
        Destination = x.Destination,
        Purpose = x.Purpose,
        CustomerOrPartner = x.CustomerOrPartner,
        TransportMethod = x.TransportMethod,
        EstimatedCost = x.EstimatedCost
    };
}
