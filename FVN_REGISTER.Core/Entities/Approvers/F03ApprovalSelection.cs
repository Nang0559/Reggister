using FVN_REGISTER.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.Approvers;

[Table("F03ApprovalSelections")]
public sealed class F03ApprovalSelection : BaseAuditEntity
{
    [Required]
    public RequestModule RequestType { get; set; }

    [Required]
    public int RequestId { get; set; }

    [Required]
    public int Level { get; set; }

    [Required, StringLength(50)]
    public string ApproverCode { get; set; } = string.Empty;
}
