using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Equipment;

[Table("F03EquipmentInspectionItems")]
public sealed class F03EquipmentInspectionItem : BaseAuditEntity
{
    public int TemplateId { get; set; }
    [Required, StringLength(60)] public string ItemCode { get; set; } = string.Empty;
    [Required, StringLength(250)] public string ItemLabel { get; set; } = string.Empty;
    [Required, StringLength(30)] public string InputType { get; set; } = "PassFail";
    public bool IsRequired { get; set; }
    public bool RequireImage { get; set; }
    public int MinImages { get; set; }
    public int MaxImages { get; set; } = 3;
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    [StringLength(30)] public string? Unit { get; set; }
    [StringLength(2000)] public string? OptionsJson { get; set; }
    public int DisplayOrder { get; set; }
}