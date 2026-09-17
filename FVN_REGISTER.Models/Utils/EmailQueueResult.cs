using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Core.Utils
{
    public class EmailQueueResult
    {
        public bool IsSuccess { get; set; }
        public bool IsTemplateNotFound { get; set; }
        public string? Message { get; set; }

        public static EmailQueueResult Ok()
            => new() { IsSuccess = true };

        public static EmailQueueResult TemplateNotFound(string templateCode)
            => new()
            {
                IsSuccess = false,
                IsTemplateNotFound = true,
                Message = $"Template email '{templateCode}' chưa được khai báo. " +
                          $"Đơn đã được tạo nhưng email thông báo chưa được gửi."
            };

        public static EmailQueueResult Fail(string message)
            => new() { IsSuccess = false, Message = message };
    }
}
