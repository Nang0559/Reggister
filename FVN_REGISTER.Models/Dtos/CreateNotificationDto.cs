using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos
{
    public class CreateNotificationDto
    {
        public int UserId { get; set; }
        public string? EmployeeCode { get; set; }
        public string NotificationType { get; set; } = "SYSTEM";
        public string Title { get; set; } = string.Empty;
        public string? Body { get; set; }
        public string? ActionUrl { get; set; }
        public int? RelatedLeaveId { get; set; }
    }
}
