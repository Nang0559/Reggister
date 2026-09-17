using System.ComponentModel.DataAnnotations;

namespace FVN_REGISTER.Contract.Requests
{
    public sealed class HistoryCancelRequestDto
    {
        [Required(ErrorMessage = "Vui lòng nhập lý do hủy")]
        public string Reason { get; set; } = string.Empty;
    }
}
