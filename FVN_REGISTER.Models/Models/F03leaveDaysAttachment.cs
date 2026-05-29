using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class F03leaveDaysAttachment
{
    public int FileId { get; set; }

    public int RequestId { get; set; }

    public string? FileName { get; set; }

    public string FilePath { get; set; } = null!;

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }
}
