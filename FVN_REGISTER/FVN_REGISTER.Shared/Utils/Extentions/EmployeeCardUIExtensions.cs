using FVN_REGISTER.Contract.Dtos.Employees;


namespace FVN_REGISTER.Shared.Utils.Extentions
{
    public static class EmployeeCardUIExtensions
    {
        /// <summary>Chữ cái đầu tên (họ tên cuối cùng) dùng cho Avatar.</summary>
        public static string ToAvatarInitial(this EmployeeCardDto dto) =>
            dto.EmployeeName.Length > 0
                ? dto.EmployeeName.Trim().Split(' ').Last()[..1].ToUpper()
                : "?";

        /// <summary>Nhãn trạng thái riêng cho nhân viên (khác nghĩa với ToActiveLabel dùng chung).</summary>
        public static string ToWorkStatusLabel(this EmployeeCardDto dto) =>
            dto.IsActive ? "Đang làm việc" : "Đã nghỉ";
    }
}
