using FVN_REGISTER.Contract.Interfaces.Emails;
using FVN_REGISTER.Contract.Interfaces.OT;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Services;

namespace FVN_REGISTER.API.Services.OT
{
    // OTNotificationService.cs
    public class OTNotificationService : BaseService<OTNotificationService>,
        IOTNotificationService
    {
        private readonly IEmailService _email;

        public async Task NotifyApprovalChainAsync(
            F03OTRequest ot, CancellationToken ct)
        {
            // Gửi cho BCH Công đoàn trước
            if (!string.IsNullOrEmpty(ot.UnionRepEmail))
                await _email.QueueEmail(ot.UnionRepEmail, "OT_REQUEST_NEW", new
                {
                    ApproverName = ot.UnionRepCode,
                    OTDate = ot.OTDate.ToString("dd/MM/yyyy"),
                    OTType = GetOTTypeName(ot.OTType),
                    TotalEmployees = ot.Rows.Count,
                    Department = ot.DeptCode,
                    CreatedBy = ot.CreatedByName,
                    Url = $"https://yourdomain/ot/approve/{ot.Id}"
                }, ct);

            // Gửi cho từng nhân viên trong danh sách
            foreach (var row in ot.Rows)
            {
                var empEmail = await GetEmpEmailAsync(row.EmployeeCode);
                if (!string.IsNullOrEmpty(empEmail))
                    await _email.QueueEmail(empEmail, "OT_EMPLOYEE_ADDED", new
                    {
                        EmployeeName = row.EmployeeName,
                        OTDate = ot.OTDate.ToString("dd/MM/yyyy"),
                        PlannedFrom = row.PlannedFrom.ToString("HH:mm"),
                        PlannedTo = row.PlannedTo.ToString("HH:mm"),
                        OTReason = row.OTReason,
                        Url = $"https://yourdomain/ot/my-row/{ot.Id}"
                    }, ct);
            }
        }

        public async Task NotifyNextApproverAsync(
            F03OTRequest ot, string level, CancellationToken ct)
        {
            // Sau khi Union approve → báo Chief
            // Sau khi Chief approve → báo MG
            // Sau khi MG approve → báo GM (nếu cần) hoặc approved
            var (email, name) = level switch
            {
                "Union" => (ot.ChiefEmail, "Ast.Chief/Chief"),
                "Chief" => (ot.MGEmail, "A.MG/MG"),
                "MG" => (ot.GMEmail, "GM"),
                _ => (null, null)
            };

            if (string.IsNullOrEmpty(email)) return;

            await _email.QueueEmail(email, "OT_REQUEST_NEW", new
            {
                ApproverName = name,
                OTDate = ot.OTDate.ToString("dd/MM/yyyy"),
                OTType = GetOTTypeName(ot.OTType),
                TotalEmployees = ot.Rows.Count,
                Department = ot.DeptCode,
                Url = $"https://yourdomain/ot/approve/{ot.Id}"
            }, ct);
        }

        public async Task NotifyResultAsync(
            F03OTRequest ot, string status, CancellationToken ct)
        {
            // Gửi kết quả cho người tạo đơn
            if (!string.IsNullOrEmpty(ot.CreatedByEmail))
                await _email.QueueEmail(ot.CreatedByEmail, "OT_STATUS_CHANGED", new
                {
                    CreatedByName = ot.CreatedByName,
                    OTDate = ot.OTDate.ToString("dd/MM/yyyy"),
                    Status = status == OTStatus.Approved ? "ĐÃ DUYỆT" : "TỪ CHỐI",
                    Comment = ot.MGComment ?? ot.ChiefComment ?? ot.GMComment
                }, ct);
        }

        private static string GetOTTypeName(string type) => type switch
        {
            "Normal" => "Ngày thường",
            "Weekend" => "Ngày nghỉ tuần",
            "Holiday" => "Ngày lễ/Tết",
            _ => type
        };
    }
}
