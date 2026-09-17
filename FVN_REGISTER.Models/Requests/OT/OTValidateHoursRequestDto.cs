using System.ComponentModel.DataAnnotations;

namespace FVN_REGISTER.Contract.Requests.OT
{
    public sealed class OTValidateHoursRequestDto
    {
        [Required]
        public string EmployeeCode { get; set; } = string.Empty;

        public DateTime OTDate { get; set; }

        [Range(0, 24)]
        public decimal Hours { get; set; }

        [Required]
        public string OTType { get; set; } = string.Empty;
    }
}
