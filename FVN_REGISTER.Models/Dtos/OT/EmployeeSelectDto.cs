using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.OT
{
    public class EmployeeSelectDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string DeptCode { get; set; } = string.Empty;
        public string DeptName { get; set; } = string.Empty;
        public string CvCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Cấp duyệt mặc định suy ra từ CvCode/PositionCode (CvCodeRules.ResolveLevel).
        /// Null nếu vị trí không tham gia luồng duyệt.
        /// </summary>
        public int? DefaultLevel { get; set; }

        /// <summary>
        /// True nếu nhân viên này đủ điều kiện làm người duyệt
        /// (CvCodeRules.IsApprover: dựa trên PositionCode + cờ IsApprove/IsAllowApprove của Position).
        /// </summary>
        public bool IsApprover { get; set; }
    }
}
