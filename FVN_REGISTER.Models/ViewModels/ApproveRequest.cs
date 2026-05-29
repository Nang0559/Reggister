using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.ViewModels
{
    public class ApproveRequest
    {
        // Danh sách các ID đơn cần phê duyệt/từ chối
        public List<int> Ids { get; set; } = new();

        // Cấp bậc duyệt (Level 1, 2 hoặc 3)
        public int Level { get; set; }

        // Ý kiến phê duyệt hoặc lý do từ chối
        public string? Comment { get; set; }
    }
}
