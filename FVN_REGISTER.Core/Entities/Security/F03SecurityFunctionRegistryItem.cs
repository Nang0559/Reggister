using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FVN_REGISTER.Core.Entities;

namespace FVN_REGISTER.Core.Entities.Security;

/// <summary>
/// Persistent discovery state for a security function declared by the application.
/// This table is intentionally separate from F03Functions so discovery never grants or removes access by itself.
/// </summary>
[Table("F03SecurityFunctionRegistry")]
public class F03SecurityFunctionRegistryItem : BaseAuditEntity
{
    [Required, StringLength(150)] public string FunctionKey { get; set; } = string.Empty;
    public int FunctionCode { get; set; }
    [Required, StringLength(150)] public string DefinitionName { get; set; } = string.Empty;
    [StringLength(50)] public string? ModuleCode { get; set; }
    [StringLength(50)] public string? ActionCode { get; set; }
    [StringLength(30)] public string? ScopeCode { get; set; }
    [Required, StringLength(30)] public string LifecycleStatus { get; set; } = "PendingRegistration";
    [Required, StringLength(30)] public string SourceType { get; set; } = "SecurityFunctionCodes";
    [StringLength(250)] public string? SourceAssembly { get; set; }
    [StringLength(250)] public string? SourceTypeName { get; set; }
    [StringLength(150)] public string? ReplacementFunctionKey { get; set; }
    [StringLength(128)] public string DefinitionHash { get; set; } = string.Empty;
    public DateTime FirstDiscoveredAt { get; set; }
    public DateTime LastSeenAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public bool IsIgnored { get; set; }
}
