using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Equipment;

[Table("F03EquipmentAssets")]
public sealed class F03EquipmentAsset : BaseAuditEntity
{
    [Required, StringLength(30)] public string EquipmentCode { get; set; } = string.Empty;
    [Required, StringLength(250)] public string EquipmentName { get; set; } = string.Empty;
    [StringLength(1000)] public string? Specification { get; set; }
    [StringLength(100)] public string? SerialNumber { get; set; }
    [StringLength(50)] public string? AssetCode { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal PurchasePrice { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime ExpectedDepreciationDate { get; set; }
    [Required, StringLength(20)] public string DeptCode { get; set; } = string.Empty;
    [StringLength(250)] public string? Location { get; set; }
    [Required, StringLength(128)] public string QrToken { get; set; } = string.Empty;
    public bool IsQrActive { get; set; }
    [StringLength(1000)] public string? Note { get; set; }
    [StringLength(50)] public string? ResponsibleEmployeeCode { get; set; }
    [StringLength(50)] public string? ResponsibleApproverEmployeeCode { get; set; }
    public DateTime? ResponsibleAssignedAt { get; set; }


    [StringLength(20)] public string? OperatingResponsibleDeptCode { get; set; }
    [StringLength(50)] public string? OperatingResponsibleEmployeeCode { get; set; }
    public DateTime? OperatingResponsibleAssignedAt { get; set; }

    // Flexible, department-specific attributes. The canonical fields above remain strongly typed.
    [Column(TypeName = "nvarchar(max)")]
    public string CustomDataJson { get; set; } = "{}";

    public ICollection<F03EquipmentRepairHistory> RepairHistory { get; set; } = new List<F03EquipmentRepairHistory>();
    public ICollection<F03EquipmentRequest> Requests { get; set; } = new List<F03EquipmentRequest>();
    public ICollection<F03EquipmentInspectionAssignment> InspectionAssignments { get; set; } = new List<F03EquipmentInspectionAssignment>();
    public ICollection<F03EquipmentInspectionTask> InspectionTasks { get; set; } = new List<F03EquipmentInspectionTask>();
    public ICollection<F03EquipmentAssignmentHistory> AssignmentHistory { get; set; } = new List<F03EquipmentAssignmentHistory>();
}