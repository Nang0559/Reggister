using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Core.Entities
{
    public interface IHistoryRequestEntity : IAuditEntity
    {
        string EmployeeCode { get; }
        ApprovalStatus RequestStatus { get; set; }
    }
}
