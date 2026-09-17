
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Interfaces;

namespace FVN_REGISTER.Contract.Dtos.Approvals
{
    public class ApprovalDetailLineDto : IValidatableRow
    {
        // ── Dữ liệu nghiệp vụ ────────────────────────
        public string? Code { get; set; }
        public string? Name { get; set; }
        public decimal Value { get; set; }

        // ── Cờ trạng thái & thông tin phụ ──────────
        public bool IsHalfDay { get; set; }
        public string? Option { get; set; }

        // ── Implement IValidatableRow ───────────────
        // Dùng Enum thay cho bool HasError để quản lý đầy đủ các trạng thái
        public RowStatus Status { get; set; } = RowStatus.Default;
        public string? Message { get; set; }

        
    }
}
