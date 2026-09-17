using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Trips;

[Table("F03TripRequests")]
public class F03TripRequest : BaseRequestEntity
{
    [Required, StringLength(30)]
    public string TripCode { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
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

    [Column(TypeName = "decimal(18,2)")]
    public decimal? EstimatedCost { get; set; }

    [StringLength(500)]
    public string? Accommodation { get; set; }

    [StringLength(1000)]
    public string? Note { get; set; }
}
