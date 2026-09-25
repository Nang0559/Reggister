using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Equipment;

[Table("F03EquipmentSchemas")]
public sealed class F03EquipmentSchema : BaseAuditEntity
{
    [Required, StringLength(20)]
    public string DeptCode { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string SchemaName { get; set; } = string.Empty;

    public int Version { get; set; }

    [Required, StringLength(20)]
    public string Status { get; set; } = "Draft";

    public ICollection<F03EquipmentFieldDefinition> Fields { get; set; } = new List<F03EquipmentFieldDefinition>();
}
