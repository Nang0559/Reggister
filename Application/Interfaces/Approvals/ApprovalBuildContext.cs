using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Application.Interfaces.Approvals
{
    // <summary>
    /// Context truyền vào BuildHierarchyAsync của mọi IApprovalProvider.
    /// 3 field cơ bản dùng chung cho tất cả domain.
    /// Extra chứa tham số đặc thù của từng domain — Provider tự đọc, Provider khác bỏ qua.
    /// </summary>
    public sealed class ApprovalBuildContext
    {
        public string EmployeeCode { get; init; } = "";
        public string DeptCode { get; init; } = "";
        public string PositionCode { get; init; } = "";

        /// <summary>
        /// Tham số domain-specific. Dùng helper property bên dưới để đọc an toàn.
        /// Ví dụ OT: Extra["TotalOTHours"] = 4.5m, Extra["OTTypeCode"] = "HOLIDAY"
        /// </summary>
        public Dictionary<string, object?> Extra { get; init; } = new();

        // ── OT helpers ────────────────────────────────────────────────
        /// <summary>Số giờ OT dự kiến — dùng để tính requireGM trong OTApprovalProvider.</summary>
        public decimal TotalOTHours =>
            Extra.TryGetValue("TotalOTHours", out var v) && v is decimal d ? d : 0m;

        /// <summary>Loại OT: WEEKDAY | WEEKEND | HOLIDAY</summary>
        public string OTTypeCode =>
            Extra.TryGetValue("OTTypeCode", out var v) && v is string s ? s : "WEEKDAY";

        // ── Leave helpers ──
        /// <summary>Năm tài chính/năm đăng ký phép</summary>
        public int Year => Extra.TryGetValue("Year", out var v) && v is int i ? i : DateTime.Today.Year;

        /// <summary>Loại phép: PhepNam | PhepKhongLuong | ...</summary>
        public string LeaveTypeCode => Extra.TryGetValue("LeaveTypeCode", out var v) && v is string s ? s : "";

        // ════════════════════════════════════════════════════════════════
        // FACTORY METHODS — tránh mỗi service tự gõ tay Dictionary với key
        // string dễ sai chính tả (không có compiler check). Dùng các factory
        // này thay vì new ApprovalBuildContext { Extra = new() { ["..."] = ... } }
        // ở mọi nơi gọi BuildHierarchyAsync.
        // ════════════════════════════════════════════════════════════════

        public static ApprovalBuildContext ForOT(
            string employeeCode, string deptCode, string positionCode,
            decimal totalOTHours, string otTypeCode) => new()
            {
                EmployeeCode = employeeCode,
                DeptCode = deptCode,
                PositionCode = positionCode,
                Extra = new Dictionary<string, object?>
                {
                    ["TotalOTHours"] = totalOTHours,
                    ["OTTypeCode"] = otTypeCode
                }
            };

        public static ApprovalBuildContext ForLeave(
            string employeeCode, string deptCode, string positionCode,
            int? year = null, string? leaveTypeCode = null) => new()
            {
                EmployeeCode = employeeCode,
                DeptCode = deptCode,
                PositionCode = positionCode,
                Extra = new Dictionary<string, object?>
                {
                    ["Year"] = year ?? DateTime.Today.Year,
                    ["LeaveTypeCode"] = leaveTypeCode ?? ""
                }
            };
    }
}
