using FVN_REGISTER.Contract.Dtos.Leaves;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Contract.Requests.Leaves
{
    public class LeaveRequestUpsertDto
    {
        // Id dùng để phân biệt Create (0) hoặc Update (Id > 0)
        public int Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string LeaveTypeCode { get; set; } = string.Empty;

        public string? Reason { get; set; }

        // Sử dụng lại OTEmployeeDto hoặc LeaveRequestDetailDto (tùy vào dự án)
        // Quan trọng: Phải dùng DTO không chứa logic UI
        public List<LeaveRequestDetailUpsertDto> Details { get; set; } = new();

      
    }
}
