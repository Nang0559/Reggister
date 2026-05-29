using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Utils
{
    public static class LeaveStatusHelper
    {
        // Kiểm tra đơn đã hoàn tất chưa
        public static bool IsApproved(string? status)
            => string.Equals(status, LeaveStatus.Approved, StringComparison.OrdinalIgnoreCase);

        // Kiểm tra đơn có đang trong luồng duyệt (chưa xong, chưa bị từ chối) hay không
        public static bool IsInProcess(string? status)
        {
            if (string.IsNullOrEmpty(status)) return true;
            // Dùng mảng ActiveStatuses bạn đã định nghĩa để kiểm tra nhanh
            return LeaveStatus.ActiveStatuses.Any(s => string.Equals(s, status, StringComparison.OrdinalIgnoreCase));
        }

        // Kiểm tra đơn bị từ chối
        public static bool IsRejected(string? status)
            => string.Equals(status, LeaveStatus.Rejected, StringComparison.OrdinalIgnoreCase);

        // Kiểm tra xem một user cụ thể có được quyền sửa/hủy đơn không (thường là khi còn Pending)
        public static bool CanEditable(string? status) => IsInProcess(status);
    }
}
