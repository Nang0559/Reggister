

using FVN_REGISTER.Core.Entities.Leaves;

namespace FVN_REGISTER.Contract.Dtos.LeaveTypes
{
    public static class LeaveTypeExtensions
    {
        // Giữ lại text hiển thị
        public static string ToTinhPhepLabel(this bool tinhPhep) =>
            tinhPhep ? "Tính phép" : "Không tính";
        public static LeaveTypeDto ToDto(this F03LeaveType entity) => new()
        {
            Id = entity.Id,
            LeaveTypeCode = entity.LeaveTypeCode,
            LeaveTypeName = entity.LeaveTypeName,
            LeaveTypeName2 = entity.LeaveTypeName2,
            IsCountedAsLeave = entity.IsCountedAsLeave,
            HRMCode = entity.HRMCode,
            IsActive = entity.IsActive == true
        };

    }
}
