using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.EmailTemplates
{
    public class EmailTemplateDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Subject { get; set; } = "";
        public string? Body { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
