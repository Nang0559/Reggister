using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Equipment;

[Table("F03EquipmentAssignmentHistory")]
public sealed class F03EquipmentAssignmentHistory : BaseAuditEntity
{
    public int AssetId { get; set; }

    [StringLength(50)] public string? PreviousResponsibleEmployeeCode { get; set; }
    [StringLength(50)] public string? NewResponsibleEmployeeCode { get; set; }
    [StringLength(50)] public string? PreviousApproverEmployeeCode { get; set; }
    [StringLength(50)] public string? NewApproverEmployeeCode { get; set; }

    [Required, StringLength(500)] public string Reason { get; set; } = string.Empty;
    public DateTime HandoverAt { get; set; }
    public int HandoverByUserId { get; set; }

    public F03EquipmentAsset Asset { get; set; } = null!;
}
