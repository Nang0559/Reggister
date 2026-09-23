using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Security;

[Table("F03ManagedScopes")]
public sealed class F03ManagedScope : BaseAuditEntity
{
    [Required, StringLength(50)]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string NodeType { get; set; } = "Department";

    [StringLength(50)]
    public string? NodeCode { get; set; }

    [StringLength(50)]
    public string? FactoryCode { get; set; }

    [StringLength(20)]
    public string? DeptCode { get; set; }

    [StringLength(20)]
    public string? SubDepartmentCode { get; set; }

    public bool IncludeChildren { get; set; } = true;

    [StringLength(500)]
    public string? Remark { get; set; }
}
