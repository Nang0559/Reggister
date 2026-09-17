



using FVN_REGISTER.Core.Constants;

namespace FVN_REGISTER.Application.Rules
{
    public static class CvCodeRules
    {
        // ── 1. Định nghĩa hằng số CVCode (Dữ liệu từ DB) ──
        public const string GiamDoc = "0001";
        public const string SubLeader = "0002";
        public const string Worker = "0003";
        public const string Chief = "0004";
        public const string Manager = "0005";
        public const string Leader = "0006";
        public const string KyThuatVien = "0007";
        public const string Staff = "0008";
        public const string SenManager = "0009";
        public const string AstChief = "0010";
        public const string AstManager = "0011";
        public const string NhanVien = "0012";

        // ── 2. Mapping trung tâm: Gắn kết CVCode - Level - Role ──
        private static readonly Dictionary<string, (int Level, string Role)> CvMapping = new()
        {
            { GiamDoc,     (ApprovalLevel.GM,        ApproverRole.GM) },
            { SubLeader,   (ApprovalLevel.SubLeader, ApproverRole.SubLeader) },
            { Chief,       (ApprovalLevel.Chief,     ApproverRole.Chief) },
            { Manager,     (ApprovalLevel.Manager,   ApproverRole.Manager) },
            { Leader,      (ApprovalLevel.SubLeader, ApproverRole.SubLeader) },
            { SenManager,  (ApprovalLevel.Manager,   ApproverRole.Manager) },
            { AstChief,    (ApprovalLevel.Chief,     ApproverRole.Chief) },
            { AstManager,  (ApprovalLevel.Manager,   ApproverRole.Manager) }
        };

        // Danh sách các CVCode bắt buộc qua bước SubLeader
        private static readonly HashSet<string> RequiresLv1 = new() { Worker };

        // ── 3. Các hàm logic (API công khai cho toàn hệ thống) ──

        /// <summary>
        /// Lấy thông tin Level và Role dựa trên CVCode.
        /// </summary>
        public static (int Level, string Role) GetApproverInfo(string cvCode)
        {
            return CvMapping.TryGetValue(cvCode, out var info) ? info : (0, string.Empty);
        }

        /// <summary>
        /// Kiểm tra quyền duyệt dựa trên trạng thái trong DB.
        /// </summary>
        public static bool IsApprover(string? cvCode, int? isApprove, int? isAllowApprove)
        {
            // Worker không được duyệt đơn người khác
            if (string.IsNullOrEmpty(cvCode) || cvCode == Worker) return false;

            return isApprove == 1 || isAllowApprove == 1;
        }

        /// <summary>
        /// Xác định LevelApprove thực tế (Ưu tiên DB, Fallback theo cấu hình).
        /// </summary>
        public static int ResolveLevel(int? levelApprove, string? cvCode)
        {
            if (levelApprove is > 0) return levelApprove.Value;

            return (cvCode != null && CvMapping.TryGetValue(cvCode, out var info))
                ? info.Level
                : 0;
        }

        /// <summary>
        /// Kiểm tra nếu đơn cần qua bước SubLeader (Lv1).
        /// </summary>
        public static bool IsLv1Required(string? cvCode)
        {
            return cvCode != null && RequiresLv1.Contains(cvCode);
        }
        /// <summary>
        /// Kiểm tra xem chức danh này có phải là nhân viên chỉ có quyền tạo đơn hay không.
        /// </summary>
        public static bool IsRequestOnly(string cvCode)
        {
            // Nếu không nằm trong Mapping (không có Level duyệt) thì là người tạo đơn thuần túy
            return !CvMapping.ContainsKey(cvCode);
        }
        /// <summary>
        /// Trả về DeptCode duyệt (xử lý trường hợp GM duyệt toàn bộ).
        /// </summary>
        public static string GetEffectiveDept(string role, string deptCode)
        {
            return role == ApproverRole.GM ? ApproveForDept.All : deptCode;
        }
    }
}
