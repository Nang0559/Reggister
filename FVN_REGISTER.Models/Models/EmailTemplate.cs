using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class EmailTemplate
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public string? Subject { get; set; }

    public string? Body { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }
}
