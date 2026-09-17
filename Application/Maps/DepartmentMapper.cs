

using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Core.Entities.HR;

namespace FVN_REGISTER.Application.Maps
    {
    public static class DepartmentMapper
    {
        public static DepartmentDto ToDto(this F03Department d, int employeeCount = 0) => new()
        {
            Id = d.Id,
            DeptCode = d.DeptCode,
            DeptName = d.DeptName,
            IsActive = d.IsActive,
            EmployeeCount = employeeCount
        };

        /// <summary>Tạo entity mới từ Upsert Dto (dùng cho Create).</summary>
        public static F03Department ToEntity(this DepartmentUpsertDto model, int currentUserId) => new()
        {
            DeptCode = model.DeptCode.Trim(),
            DeptName = model.DeptName.Trim(),
            IsActive = model.IsActive,
            CreatedBy = currentUserId,
            CreatedAt = DateTime.Now,
            ModifiedBy = currentUserId,
            ModifiedAt = DateTime.Now
        };

        /// <summary>Áp Upsert Dto lên entity đã có sẵn (dùng cho Update).</summary>
        public static void ApplyTo(this DepartmentUpsertDto model, F03Department entity, int currentUserId)
        {
            entity.DeptCode = model.DeptCode.Trim();
            entity.DeptName = model.DeptName.Trim();
            entity.IsActive = model.IsActive;
            entity.ModifiedBy = currentUserId;
            entity.ModifiedAt = DateTime.Now;
        }
    }
}

