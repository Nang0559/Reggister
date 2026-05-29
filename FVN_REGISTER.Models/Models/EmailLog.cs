using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class EmailLog
{
    public int Id { get; set; }

    public int? QueueId { get; set; }

    public string? ToEmail { get; set; }

    public string? Subject { get; set; }

    public string? Status { get; set; }

    public DateTime? SentAt { get; set; }

    public string? ErrorMessage { get; set; }
}
