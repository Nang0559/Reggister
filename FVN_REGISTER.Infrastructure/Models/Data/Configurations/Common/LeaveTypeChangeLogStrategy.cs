using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Core.Entities.HRM;
using FVN_REGISTER.Core.Entities.Leaves;
using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Common
{
    public sealed class LeaveTypeChangeLogStrategy
     : IHrmChangeLogStrategy<string, HrmLeaveTypeChangeLog, F03LeaveType>
    {
        public string GetKey(HrmLeaveTypeChangeLog change) => change.LeaveTypeCode;
        public string GetEntityKey(F03LeaveType entity) => entity.LeaveTypeCode;
        public HrmChangeAction GetAction(HrmLeaveTypeChangeLog change) => change.ActionType;

        public F03LeaveType MapToNewEntity(HrmLeaveTypeChangeLog change) => new()
        {
            LeaveTypeCode = change.LeaveTypeCode,
            LeaveTypeName = change.LeaveTypeName ?? "",
            LeaveTypeName2 = change.LeaveTypeName2,
            TinhPhep = change.TinhPhep ?? false,
            HRMCode = change.HRMCode,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        public void ApplyUpdate(F03LeaveType entity, HrmLeaveTypeChangeLog change)
        {
            entity.LeaveTypeName = change.LeaveTypeName ?? entity.LeaveTypeName;
            entity.LeaveTypeName2 = change.LeaveTypeName2;
            entity.TinhPhep = change.TinhPhep ?? entity.TinhPhep;
            entity.HRMCode = change.HRMCode;
            entity.ModifiedAt = DateTime.Now;
            entity.ModifiedBy = -1;
        }

        public void ApplyDelete(F03LeaveType entity)
        {
            entity.IsActive = false;
            entity.ModifiedAt = DateTime.Now;
            entity.ModifiedBy = -1;
        }
    }
}
