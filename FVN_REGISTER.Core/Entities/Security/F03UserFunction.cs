
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Security;

[Table("F03UserFunctions")]
public partial class F03UserFunction
{
    [Key]
    public int Id { get; set; }

    public int IdUser { get; set; }
    public int IdPermission { get; set; }
    public int IdFunction { get; set; }

    // Navigation Properties
    [ForeignKey("IdUser")]
    public virtual F03User User { get; set; } = null!;

    [ForeignKey("IdPermission")]
    public virtual F03Permission Permission { get; set; } = null!;

    [ForeignKey("IdFunction")]
    public virtual F03Function Function { get; set; } = null!; // Đã cập nhật kết nối này
}
