using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class F03registerType
{
    public int RegisterId { get; set; }

    public string RegisterName { get; set; } = null!;

    public string? HrmregisterCode { get; set; }

    public decimal? TotalDay { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }
}
