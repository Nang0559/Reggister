using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Core.Constants
{
    public static class EmployeeStatusConst
    {
        public const string Present = "PRESENT";
        public const string OnLeave = "ON_LEAVE";
        public const string Absent = "ABSENT";   // không chấm công, không có phép

        public static string GetDisplay(string status) => status switch
        {
            Present => "Có mặt",
            OnLeave => "Nghỉ phép",
            Absent => "Vắng không lý do",
            _ => "Không xác định"
        };

        public static string GetColor(string status) => status switch
        {
            Present => "#4CAF50",  // Green
            OnLeave => "#2196F3",  // Blue
            Absent => "#F44336",  // Red
            _ => "#9E9E9E"
        };
    }
}
