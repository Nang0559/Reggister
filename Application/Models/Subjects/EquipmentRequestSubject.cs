using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Core.Entities.Equipment;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Models.Subjects;

public sealed class EquipmentRequestSubject : IApprovalSubject
{
    public int RequestId { get; set; }
    public RequestModule Module => RequestModule.Equipment;
    public string EmployeeCode { get; set; } = string.Empty;
    public string? EmployeeName { get; set; }
    public string? DeptCode { get; set; }
    public string? PositionCode { get; set; }
    public ApprovalStatus OverallStatus { get; set; }
    public EquipmentRequestKind RequestKind { get; set; }
    public int? AssetId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string? AssetCode { get; set; }
    public string SelectedApproverCode { get; set; } = string.Empty;
    public DateTime? RepairDate { get; set; }
    public decimal? Amount { get; set; }

    public static EquipmentRequestSubject From(F03EquipmentRequest x, string? employeeName = null, string? positionCode = null) => new()
    {
        RequestId = x.Id,
        EmployeeCode = x.EmployeeCode,
        EmployeeName = employeeName,
        DeptCode = x.DeptCode,
        PositionCode = positionCode,
        OverallStatus = x.RequestStatus,
        RequestKind = x.RequestKind,
        AssetId = x.AssetId,
        EquipmentName = x.EquipmentName,
        AssetCode = x.AssetCode,
        SelectedApproverCode = x.SelectedApproverCode,
        RepairDate = x.RepairDate,
        Amount = x.RequestKind == EquipmentRequestKind.Repair ? x.RepairCost : x.PurchasePrice
    };
}
