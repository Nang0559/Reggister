using FVN_REGISTER.Contract.Dtos.LeaveTypes;
using FVN_REGISTER.Core.Entities.Leaves;


namespace FVN_REGISTER.Application.Maps
{
    public static class LeaveTypeMapper
    {
        public static LeaveTypeDto ToDto(F03LeaveType e) => new()
        {
            Id = e.Id,
            LeaveTypeCode = e.LeaveTypeCode,
            LeaveTypeName = e.LeaveTypeName,
            LeaveTypeName2 = e.LeaveTypeName2,
            IsCountedAsLeave = e.IsCountedAsLeave == true,
            HRMCode = e.HRMCode,
            IsActive = e.IsActive == true
        };

        public static void ApplyTo(F03LeaveType entity, LeaveTypeUpsertDto dto, int actorUserId)
        {
            entity.LeaveTypeCode = dto.LeaveTypeCode;
            entity.LeaveTypeName = dto.LeaveTypeName;
            entity.LeaveTypeName2 = dto.LeaveTypeName2;
            entity.IsCountedAsLeave = dto.IsCountedAsLeave;
            entity.HRMCode = dto.HRMCode;
            entity.IsActive = dto.IsActive;
            entity.ModifiedBy = actorUserId;
            entity.ModifiedAt = DateTime.Now;
        }
    }
}
