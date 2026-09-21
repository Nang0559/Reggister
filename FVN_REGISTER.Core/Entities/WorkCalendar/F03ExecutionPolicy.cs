using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FVN_REGISTER.Core.Entities.WorkCalendar;
[Table("F03ExecutionPolicies")]
public sealed class F03ExecutionPolicy : BaseAuditEntity
{
    public new int Id { get; set; }
    [Required, StringLength(50)] public string ModuleCode { get; set; } = string.Empty;
    public byte ReconciliationMode { get; set; } = 1;
    public byte ConfirmationMode { get; set; }
    public byte EvidenceMode { get; set; } = 2;
    public byte ReviewMode { get; set; } = 1;
    public int? DueHours { get; set; }
    public byte AutoResolveMode { get; set; }
    public byte CorrectionMode { get; set; }
}