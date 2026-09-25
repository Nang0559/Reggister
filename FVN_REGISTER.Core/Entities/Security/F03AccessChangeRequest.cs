using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Core.Entities.Security;

[Table("F03AccessChangeRequests")]
public sealed class F03AccessChangeRequest : BaseAuditEntity
{
    [Required] public RequestModule BusinessModule { get; set; }
    [Required, StringLength(50)] public string RequesterEmployeeCode { get; set; } = string.Empty;
    [Required, StringLength(50)] public string OldEmployeeCode { get; set; } = string.Empty;
    [Required, StringLength(50)] public string NewEmployeeCode { get; set; } = string.Empty;
    [Required, StringLength(20)] public string DeptCode { get; set; } = string.Empty;
    [StringLength(20)] public string? NewPositionCode { get; set; }
    [Required, StringLength(500)] public string Reason { get; set; } = string.Empty;
    [Column(TypeName = "nvarchar(max)")] public string RequestedFunctionCodesJson { get; set; } = "[]";
    [Column(TypeName = "nvarchar(max)")] public string EquipmentAssetIdsJson { get; set; } = "[]";
    [StringLength(50)] public string? OldEquipmentResponsibleCode { get; set; }
    [StringLength(50)] public string? NewEquipmentResponsibleCode { get; set; }
    [StringLength(50)] public string? OldEquipmentApproverCode { get; set; }
    [StringLength(50)] public string? NewEquipmentApproverCode { get; set; }
    [Column(TypeName = "nvarchar(max)")] public string? PreChangeSnapshotJson { get; set; }
    [Column(TypeName = "nvarchar(max)")] public string? PostChangeResultJson { get; set; }
    public AccessChangeStatus Status { get; set; } = AccessChangeStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ITCompletedAt { get; set; }
    public int? ITCompletedByUserId { get; set; }
    [StringLength(1000)] public string? ITNote { get; set; }
}
