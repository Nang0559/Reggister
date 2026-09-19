using FVN_REGISTER.Contract.Requests.Approvals;
using System.ComponentModel.DataAnnotations;

namespace FVN_REGISTER.Contract.Dtos.Trips;

public sealed class CreateTripRequestDto
{
    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required, StringLength(250)]
    public string Destination { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string Purpose { get; set; } = string.Empty;

    [StringLength(250)]
    public string? CustomerOrPartner { get; set; }

    [StringLength(100)]
    public string? TransportMethod { get; set; }

    [StringLength(500)]
    public string? CompanionEmployeeCodes { get; set; }

    [Range(0, 999999999)]
    public decimal? EstimatedCost { get; set; }

    [StringLength(500)]
    public string? Accommodation { get; set; }

    [StringLength(1000)]
    public string? Note { get; set; }
        public List<undefined> undefined { get; set; } = new();

}
