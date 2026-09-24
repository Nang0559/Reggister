using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Equipment;

[Table("F03EquipmentInspectionTemplates")]
public sealed class F03EquipmentInspectionTemplate : BaseAuditEntity
{
    [Required, StringLength(50)] public string TemplateCode { get; set; } = string.Empty;
    [Required, StringLength(200)] public string TemplateName { get; set; } = string.Empty;
    [Required, StringLength(20)] public string DeptCode { get; set; } = string.Empty;
    [Required, StringLength(20)] public string Frequency { get; set; } = "Daily";
    public int Version { get; set; } = 1;
    [Required, StringLength(20)] public string Status { get; set; } = "Draft";
    public string? Description { get; set; }
    public ICollection<F03EquipmentInspectionItem> Items { get; set; } = new List<F03EquipmentInspectionItem>();
    public ICollection<F03EquipmentInspectionAssignment> Assignments { get; set; } = new List<F03EquipmentInspectionAssignment>();
}