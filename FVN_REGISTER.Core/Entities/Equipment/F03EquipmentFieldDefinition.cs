using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Equipment;

[Table("F03EquipmentFieldDefinitions")]
public sealed class F03EquipmentFieldDefinition : BaseAuditEntity
{
    public int SchemaId { get; set; }

    [Required, StringLength(20)] public string DeptCode { get; set; } = string.Empty;
    [Required, StringLength(60)] public string FieldKey { get; set; } = string.Empty;
    [Required, StringLength(150)] public string FieldLabel { get; set; } = string.Empty;
    [Required, StringLength(20)] public string DataType { get; set; } = "Text";
    public bool IsRequired { get; set; }
    public bool IsImportable { get; set; } = true;
    public bool IsSearchable { get; set; }
    public bool IsActiveField { get; set; } = true;
    public int DisplayOrder { get; set; }
    public int? MaxLength { get; set; }
    [StringLength(500)] public string? DefaultValue { get; set; }
    [StringLength(2000)] public string? OptionsJson { get; set; }

    public F03EquipmentSchema? Schema { get; set; }
}
