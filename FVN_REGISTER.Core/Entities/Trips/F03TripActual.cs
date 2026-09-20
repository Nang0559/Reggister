using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Trips;

/// <summary>
/// Immutable execution baseline created when a trip request becomes Approved.
/// The initial actual dates intentionally equal the approved registration dates.
/// A change of traveller is a new TripRequest; this record is never reassigned
/// to another employee.
/// </summary>
[Table("F03TripActual")]
public sealed class F03TripActual : BaseAuditEntity
{
    public int TripRequestId { get; set; }

    [Required, StringLength(30)]
    public string TripCode { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string EmployeeCode { get; set; } = string.Empty;

    public DateTime ActualStartDate { get; set; }
    public DateTime ActualEndDate { get; set; }

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

    [StringLength(500)]
    public string? Accommodation { get; set; }

    [StringLength(1000)]
    public string? Note { get; set; }

    /// <summary>
    /// Scheduled, InProgress, Completed, Cancelled.
    /// </summary>
    [Required, StringLength(30)]
    public string ActualStatus { get; set; } = "Scheduled";

    public DateTime ApprovedAt { get; set; }
}
