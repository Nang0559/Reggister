
using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Contract.Dtos.LeaveTypes
{
    public class LeaveTypeUpsertDto
    {
        // 0 khi tạo mới, > 0 khi cập nhật
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã loại nghỉ không được để trống")]
        [MaxLength(50)]
        public string LeaveTypeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên loại nghỉ không được để trống")]
        [MaxLength(200)]
        public string LeaveTypeName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? LeaveTypeName2 { get; set; }

        public bool IsCountedAsLeave { get; set; }

        [MaxLength(50)]
        public string? HRMCode { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
