


using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Interfaces.Approvals
{
    public interface IApprovalSubject
    {
        int RequestId { get; }
        RequestModule Module { get; } // ĐÃ THÊM
        string EmployeeCode { get; }
        string? EmployeeName { get; }
        string? DeptCode { get; }
        string? PositionCode { get; }
        // Bổ sung thêm nếu cần:
        ApprovalStatus OverallStatus { get; }
        int? OverrideLevel => null;
    }
}
