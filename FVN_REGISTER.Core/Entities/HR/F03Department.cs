
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.HR;

[Table("F03Departments")]
public partial class F03Department : BaseAuditEntity
{


    [Required, StringLength(20)]
    public string DeptCode { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string DeptName { get; set; } = string.Empty;

    // Persisted HR master field used by OT Block-scope rules.
    [StringLength(20)]
    public string? BlockCode { get; set; }
    [StringLength(50)]
  
    public string? ParentDeptCode { get; set; }   // từ BPMaCha

    public int? DisplayPriority { get; set; }     // từ BPUuTien
    public bool ShowInReport { get; set; } = true; // từ BPHienThiBC
}
