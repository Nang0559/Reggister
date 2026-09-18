using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Equipment;

[Table("F03EquipmentImportBatches")]
public sealed class F03EquipmentImportBatch : BaseAuditEntity
{
    [Required, StringLength(20)] public string DeptCode { get; set; } = string.Empty;
    [Required, StringLength(260)] public string FileName { get; set; } = string.Empty;
    [Required, StringLength(30)] public string Status { get; set; } = "Staged";
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public int ImportedRows { get; set; }
    public DateTime? CompletedAt { get; set; }
}
