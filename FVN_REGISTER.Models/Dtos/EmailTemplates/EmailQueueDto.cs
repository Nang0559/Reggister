using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.EmailTemplates
{
    public class EmailQueueDto
    {
        public int Id { get; set; }
        public string? ToEmail { get; set; }
        public string? TemplateCode { get; set; }
        public string? Payload { get; set; }
        public string? Status { get; set; }
        public int? RetryCount { get; set; }
        public int? MaxRetry { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
    }
}
