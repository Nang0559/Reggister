using System.ComponentModel.DataAnnotations;

namespace FVN_REGISTER.Contract.Requests.Leaves
{
    public sealed class LeaveCancelRequestDto
    {
        [Required(ErrorMessage = "Vui lòng nhập lý do hủy")]
        public string Reason { get; set; } = string.Empty;
    }
}
