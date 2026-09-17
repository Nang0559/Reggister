
using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Contract.Dtos.Positions
{
    public class PositionUpsertDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nhập mã chức vụ")]
        [StringLength(20)]
        public string PositionCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhập tên chức vụ")]
        [StringLength(100)]
        public string PositionName { get; set; } = string.Empty;

        public bool IsApprove { get; set; }
        public bool IsAllowApprove { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
