using System.ComponentModel.DataAnnotations;

namespace FVN_REGISTER.Contract.Requests
{
    public class CancelRequestCommandDto
    {
        // Sử dụng Id duy nhất (khóa chính) là đủ
        public string RequestId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lý do hủy")]
        public string Reason { get; set; } = string.Empty;
    }
}
