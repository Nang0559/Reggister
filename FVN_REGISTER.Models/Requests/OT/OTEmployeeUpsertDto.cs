
using System.ComponentModel.DataAnnotations;


namespace FVN_REGISTER.Contract.Requests.OT
{
    /// <summary>
    /// Input ghi — tách khỏi Dtos/OT/OTEmployeeDto.cs (read model, có field UI như
    /// ValidationStatus không nên nhận từ client).
    /// </summary>
    public class OTEmployeeUpsertDto
    {
        [Required, StringLength(50)]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required, Range(0.1, 24)]
        public decimal OTHours { get; set; }

        public string? Reason { get; set; }
    }
}
