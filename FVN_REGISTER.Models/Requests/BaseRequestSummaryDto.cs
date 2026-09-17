using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Core.Enums;
using System.Text.Json.Serialization;

namespace FVN_REGISTER.Contract.Requests;

[JsonDerivedType(typeof(OTSummaryDto), typeDiscriminator: "OT")]
[JsonDerivedType(typeof(LeaveSummaryDto), typeDiscriminator: "LEAVE")]
public abstract class BaseRequestSummaryDto
{
    public int Id { get; set; }
    public string RequestCode { get; set; } = string.Empty;
    public RequestModule RequestType { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public ApprovalStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? EmployeeName { get; set; }
    public abstract string Kind { get; }
}
