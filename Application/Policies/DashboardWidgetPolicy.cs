using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Core.Constants;


namespace FVN_REGISTER.Application.Policies
{
   

    /// <summary>
    /// Sắp xếp/giới hạn danh sách Widget trước khi trả về UI — THUẦN LOGIC, không I/O.
    /// KHÔNG chịu trách nhiệm ẩn/hiện widget theo QUYỀN (đó là việc của từng
    /// ModuleDashboardProvider — vd LeaveDashboardProvider chỉ add dept-widgets
    /// nếu user.Permission.IsApprover()). Policy chỉ lo TRÌNH BÀY những gì
    /// Provider đã quyết định trả về.
    /// </summary>
    public static class DashboardWidgetPolicy
    {
        /// <summary>Số widget tối đa hiển thị cho user thường — tránh dashboard quá tải với người không cần xem nhiều.</summary>
        private const int MaxWidgetsForNormalUser = 6;

        public static List<WidgetCounterDto> Arrange(
            UserIdentityDto user, IEnumerable<WidgetCounterDto> rawWidgets)
        {
            var ordered = rawWidgets
                // 1. Cảnh báo (Error/Warning) luôn lên đầu — bất kể vai trò, ai cũng
                //    cần thấy cảnh báo trước (VD "vượt giới hạn giờ OT", "phòng ban
                //    thiếu approver") ngay khi mở Dashboard.
                .OrderByDescending(w => w.Color is "Error" or "Warning")
                // 2. Trong cùng nhóm severity, widget cá nhân (Link trỏ về chính user,
                //    không phải trang quản lý) lên trước widget quản lý — ưu tiên
                //    thông tin "của tôi" trước thông tin "của phòng ban".
                .ThenByDescending(w => IsPersonalWidget(w))
                .ThenBy(w => w.Title)
                .ToList();

            // 3. User thường (không phải Approver/Admin) không cần thấy quá nhiều
            //    widget quản lý dồn vào — giới hạn số lượng để dashboard gọn.
            //    Approver/Admin xem đầy đủ (Provider đã tự quyết định widget nào
            //    được đưa vào rawWidgets dựa theo quyền, Policy KHÔNG lọc lại).
            //if (!user.Permission.IsApprover())
            //    return ordered.Take(MaxWidgetsForNormalUser).ToList();

            return ordered;
        }

        /// <summary>
        /// Suy đoán widget "cá nhân" hay "quản lý" dựa vào Link — widget cá nhân thường
        /// trỏ về trang history/balance của chính user, widget quản lý trỏ về trang
        /// duyệt/thống kê phòng ban. Đây là heuristic đơn giản dựa trên convention Link
        /// hiện có, KHÔNG cần thêm field phân loại mới vào WidgetCounterDto.
        /// </summary>
        private static bool IsPersonalWidget(WidgetCounterDto w)
            => w.Link != null &&
               (w.Link.Contains("/history", StringComparison.OrdinalIgnoreCase) ||
                w.Link.Contains("/balance", StringComparison.OrdinalIgnoreCase));
    }
}
