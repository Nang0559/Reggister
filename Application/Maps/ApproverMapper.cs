


using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Entities.Common;

namespace FVN_REGISTER.Application.Maps
{
    public static class ApproverMapper
    {
        // 1. Entity -> DTO
        public static ApproverDto ToDto(F03Approver a) => new()
        {
            Id = a.Id,
            RequestType = a.RequestType,
            ApproverCode = a.ApproverCode,
            ApproverName = a.ApproverName,
            ApproverEmail = a.ApproverEmail,
            Level = a.Level,
            RoleName = a.RoleName,
            PositionCode = a.PositionCode,
            DeptCode = a.ApproverDeptCode,
            DeptName = a.ApproverDeptName,
            ApproveForDeptCode = a.ApproveForDeptCode,
            ApproveForDeptName = a.ApproveForDeptName,
            IsActive = a.IsActive ?? true
        };
        public static F03Approver ToEntity(ApproverDto dto, int currentUserId) => new()
        {
            RequestType = dto.RequestType,
            ApproverCode = dto.ApproverCode,
            ApproverName = dto.ApproverName,
            ApproverEmail = dto.ApproverEmail,
            Level = dto.Level,
            RoleName = dto.RoleName,
            PositionCode = dto.PositionCode,
            ApproverDeptCode = dto.DeptCode,
            ApproverDeptName = dto.DeptName ?? string.Empty,
            ApproveForDeptCode = dto.ApproveForDeptCode,
            ApproveForDeptName = dto.ApproveForDeptName ?? string.Empty,
            IsActive = true,
            // CreatedAt/CreatedBy thường được SaveChangesAsync/Audit-interceptor của DbContext
            // tự stamp (giống ghi chú ở ApplyUpdate). Vẫn set CreatedBy tường minh ở đây để
            // không phụ thuộc vào việc interceptor có đọc được current-user hay không.
            CreatedBy = currentUserId
        };

        // 2. Map từ Cấu hình người duyệt (F03Approver) sang bản chụp (F03ApprovalStepSnapshot)
        // Đây là hàm quan trọng nhất để Orchestrator gọi khi nộp đơn
        public static F03ApprovalStepSnapshot ToSnapshotStep(F03Approver approver, bool isRequired) => new()
        {
            Level = approver.Level,
            ApproverCode = approver.ApproverCode,
            ApproverName = approver.ApproverName,
            RoleName = approver.RoleName,
            IsRequired = isRequired
        };

        // 3. Cập nhật Entity
        public static void ApplyUpdate(F03Approver entity, ApproverDto dto, int userId)
        {
            entity.ApproverName = dto.ApproverName;
            entity.ApproverEmail = dto.ApproverEmail;
            entity.Level = dto.Level;
            entity.RoleName = dto.RoleName;
            entity.ApproveForDeptCode = dto.ApproveForDeptCode;
            entity.ApproveForDeptName = dto.ApproveForDeptName ?? string.Empty;
            entity.IsActive = dto.IsActive;
            // Các trường Audit sẽ được ghi đè bởi SaveChangesAsync trong DbContext
            entity.ModifiedBy = userId;
        }
    }
}
