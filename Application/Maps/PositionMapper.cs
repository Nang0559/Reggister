using FVN_REGISTER.Contract.Dtos.Positions;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.HR;


namespace FVN_REGISTER.Application.Maps
{
    public static class PositionMapper
    {
        public static PositionDto ToDto(this F03Position p, int employeeCount = 0) => new()
        {
            Id = p.Id,
            PositionCode = p.PositionCode,
            PositionName = p.PositionName,
            IsApprove = p.IsApprove,
            IsAllowApprove = p.IsAllowApprove,
            IsActive = p.IsActive ?? false,
            EmployeeCount = employeeCount
        };

        /// <summary>Tạo entity mới từ Upsert Dto (dùng cho Create).</summary>
        public static F03Position ToEntity(this PositionUpsertDto model, int currentUserId) => new()
        {
            PositionCode = model.PositionCode.Trim(),
            PositionName = model.PositionName.Trim(),
            IsApprove = model.IsApprove,
            IsAllowApprove = model.IsAllowApprove,
            IsActive = model.IsActive,
            CreatedBy = currentUserId,
            LastModifiedSource = SyncSourceTags.Manual,
            CreatedAt = DateTime.Now
        };

        /// <summary>Áp Upsert Dto lên entity đã có sẵn (dùng cho Update).</summary>
        public static void ApplyTo(this PositionUpsertDto model, F03Position entity, int currentUserId)
        {
            entity.PositionCode = model.PositionCode.Trim();
            entity.PositionName = model.PositionName.Trim();
            entity.IsApprove = model.IsApprove;
            entity.IsAllowApprove = model.IsAllowApprove;
            entity.IsActive = model.IsActive;
            entity.ModifiedBy = currentUserId;
            entity.ModifiedAt = DateTime.Now;
        }
    }
}
