using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Application.Interfaces.Approvals;

public sealed class ApprovalBuildContext
{
    public int RequestId { get; init; }
    public string EmployeeCode { get; init; } = "";
    public string DeptCode { get; init; } = "";
    public string PositionCode { get; init; } = "";
    public Dictionary<string, object?> Extra { get; init; } = new();

    public decimal TotalOTHours => Extra.TryGetValue("TotalOTHours", out var v) && v is decimal d ? d : 0m;
    public string OTTypeCode => Extra.TryGetValue("OTTypeCode", out var v) && v is string s ? s : "WEEKDAY";
    public int Year => Extra.TryGetValue("Year", out var v) && v is int i ? i : DateTime.Today.Year;
    public string LeaveTypeCode => Extra.TryGetValue("LeaveTypeCode", out var v) && v is string s ? s : "";

    public static ApprovalBuildContext ForOT(
        int requestId,
        string employeeCode,
        string deptCode,
        string positionCode,
        decimal totalOTHours,
        string otTypeCode) => new()
    {
        RequestId = requestId,
        EmployeeCode = employeeCode,
        DeptCode = deptCode,
        PositionCode = positionCode,
        Extra = new Dictionary<string, object?>
        {
            ["TotalOTHours"] = totalOTHours,
            ["OTTypeCode"] = otTypeCode
        }
    };

    public static ApprovalBuildContext ForLeave(
        int requestId,
        string employeeCode,
        string deptCode,
        string positionCode,
        int? year = null,
        string? leaveTypeCode = null) => new()
    {
        RequestId = requestId,
        EmployeeCode = employeeCode,
        DeptCode = deptCode,
        PositionCode = positionCode,
        Extra = new Dictionary<string, object?>
        {
            ["Year"] = year ?? DateTime.Today.Year,
            ["LeaveTypeCode"] = leaveTypeCode ?? ""
        }
    };

    public static ApprovalBuildContext ForTrip(
        int requestId,
        string employeeCode,
        string deptCode,
        string positionCode) => new()
    {
        RequestId = requestId,
        EmployeeCode = employeeCode,
        DeptCode = deptCode,
        PositionCode = positionCode
    };

    public static ApprovalBuildContext ForEquipment(
        int requestId,
        string employeeCode,
        string deptCode,
        string positionCode) => new()
    {
        RequestId = requestId,
        EmployeeCode = employeeCode,
        DeptCode = deptCode,
        PositionCode = positionCode
    };
}
