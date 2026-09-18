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
            RequestType = a.RequestType.ToString(),
            ApproverCode = a.ApproverCode ?? string.Empty,
            ApproverName = a.ApproverName ?? string.Empty,
            ApproverEmail = a.ApproverEmail ?? string.Empty,
            Level = a.Level,
            RoleName = a.RoleName ?? string.Empty,
            PositionCode = a.PositionCode ?? string.Empty,
            DeptCode = a.ApproverDeptCode ?? string.Empty,
            DeptName = a.ApproverDeptName ?? string.Empty,
            ApproveForDeptCode = a.ApproveForDeptCode ?? string.Empty,
            ApproveForDeptName = a.ApproveForDeptName ?? string.Empty,
            IsActive = a.IsActive ?? true
        };

        public static F03Approver ToEntity(ApproverDto dto, int currentUserId) => new()
        {
            RequestType = dto.RequestType ?? string.Empty,
            ApproverCode = dto.ApproverCode ?? string.Empty,
            ApproverName = dto.ApproverName ?? string.Empty,
            ApproverEmail = dto.ApproverEmail ?? string.Empty,
            Level = dto.Level,
            RoleName = dto.RoleName ?? string.Empty,
            PositionCode = dto.PositionCode ?? string.Empty,
            ApproverDeptCode = dto.DeptCode ?? string.Empty,
            ApproverDeptName = dto.DeptName ?? string.Empty,
            ApproveForDeptCode = dto.ApproveForDeptCode ?? string.Empty,
            ApproveForDeptName = dto.ApproveForDeptName ?? string.Empty,
            IsActive = true,
            CreatedBy = currentUserId
        };

        // 2. Map từ Cấu hình người duyệt (F03Approver) sang bản chụp (F03ApprovalStepSnapshot)
        public static F03ApprovalStepSnapshot ToSnapshotStep(F03Approver approver, bool isRequired) => new()
        {
            Level = approver.Level,
            ApproverCode = approver.ApproverCode ?? string.Empty,
            ApproverName = approver.ApproverName ?? string.Empty,
            RoleName = approver.RoleName ?? string.Empty,
            IsRequired = isRequired
        };

        // 3. Cập nhật Entity
        public static void ApplyUpdate(F03Approver entity, ApproverDto dto, int userId)
        {
            entity.ApproverName = dto.ApproverName ?? string.Empty;
            entity.ApproverEmail = dto.ApproverEmail ?? string.Empty;
            entity.Level = dto.Level;
            entity.RoleName = dto.RoleName ?? string.Empty;
            entity.ApproveForDeptCode = dto.ApproveForDeptCode ?? string.Empty;
            entity.ApproveForDeptName = dto.ApproveForDeptName ?? string.Empty;
            entity.IsActive = dto.IsActive;
            entity.ModifiedBy = userId;
        }
    }
}
