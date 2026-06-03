using Azure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Interfaces.OT
{
    public interface IOTNotificationService
    {
        // Gửi cho approver khi có đơn mới
        Task SendApprovalRequestAsync(string approverEmail, string approverName,
                                      OTRequest ot, string submitterName, CancellationToken ct);
        // Gửi cho nhân viên trong danh sách OT
        Task SendOTAddedNotificationAsync(OTEmployeeRow row, OTRequest ot, CancellationToken ct);
        // Gửi kết quả duyệt/từ chối
        Task SendStatusChangedAsync(string targetEmail, string status,
                                    OTRequest ot, CancellationToken ct);
    }
}
