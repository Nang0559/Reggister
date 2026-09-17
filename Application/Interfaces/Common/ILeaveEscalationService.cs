using FVN_REGISTER.Application.Interfaces.Approvals;


namespace FVN_REGISTER.Application.Interfaces.Common
{
    public interface ILeaveEscalationService : IApprovalEscalationService
    {
        // Bạn có thể để trống nếu chỉ cần kế thừa từ IApprovalEscalationService
        // Hoặc thêm các phương thức riêng biệt cho nghiệp vụ Nghỉ phép (nếu có)
    }
}
