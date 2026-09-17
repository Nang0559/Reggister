

namespace FVN_REGISTER.Application.Interfaces.Users
{
    /// <summary>
    /// Tra cứu UserId (int, dùng cho SignalR/in-app notification) từ EmployeeCode (string).
    /// Tách riêng khỏi ICurrentUserService vì đây là tra cứu NGƯỜI KHÁC, không phải user hiện tại.
    /// </summary>
    public interface IEmployeeUserResolver
    {
        /// <summary>Trả null nếu EmployeeCode không có tài khoản hệ thống (F03User) — 
        /// ví dụ approver chỉ tồn tại trong HRM chưa từng đăng nhập app.</summary>
        Task<int?> ResolveUserIdAsync(string employeeCode, CancellationToken ct = default);

        /// <summary>Batch version — dùng khi cần resolve nhiều approver cùng lúc (vd Approve hàng loạt).</summary>
        Task<Dictionary<string, int>> ResolveUserIdsAsync(
            IEnumerable<string> employeeCodes, CancellationToken ct = default);
    }
}
