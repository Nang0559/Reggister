


using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Contract.Dtos.Approvals
{
    public class ApproverDto
    {
        public int Id { get; set; }
        public RequestModule RequestType { get; set; }

        // Gom nhóm thông tin để code gọn hơn
        public string ApproverCode { get; set; } = string.Empty;
        public string ApproverName { get; set; } = string.Empty;
        public string ApproverEmail { get; set; } = string.Empty;

        public int Level { get; set; }
        public string? RoleName { get; set; }
        public string? PositionCode { get; set; }

        public string DeptCode { get; set; } = string.Empty;
        public string DeptName { get; set; } = string.Empty;

        public string? ApproveForDeptCode { get; set; }
        public string? ApproveForDeptName { get; set; }

        public bool IsActive { get; set; } = true;
     
    }
}
