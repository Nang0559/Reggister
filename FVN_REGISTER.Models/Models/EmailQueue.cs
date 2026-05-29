using System;
using System.Collections.Generic;

namespace FVN_REGISTER.Contract.Models;

public partial class EmailQueue
{
    public int Id { get; set; }

    public string? ToEmail { get; set; }

    public string? Subject { get; set; }

    public string? Body { get; set; }

    public string? TemplateCode { get; set; }

    public string? Payload { get; set; }

    public string? Status { get; set; }

    public int? RetryCount { get; set; }

    public int? MaxRetry { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? SentAt { get; set; }
}
