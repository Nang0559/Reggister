
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

    // Legacy HR source field; F03Departments does not contain this column in the application DB.
    // Keep the CLR property for compatibility, but never map it to SQL.
    [NotMapped]
    [StringLength(20)]
    public string? BlockCode { get; set; }
    [StringLength(50)]
  
    public string? ParentDeptCode { get; set; }   // từ BPMaCha

    public int? DisplayPriority { get; set; }     // từ BPUuTien
    public bool ShowInReport { get; set; } = true; // từ BPHienThiBC
}
