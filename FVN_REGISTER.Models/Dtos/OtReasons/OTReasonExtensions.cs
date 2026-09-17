using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.OtReasons
{
    public static class OtReasonExtensions
    {
        // Giúp tìm nhanh tên lý do từ danh sách đã lấy từ DB
        public static string ToDisplayName(this string code, IEnumerable<OTReasonCodeDto> reasonList)
        {
            return reasonList.FirstOrDefault(x => x.ReasonCode == code)?.DisplayName ?? code;
        }

        // Tạo option cho MudSelect
        public static IEnumerable<OTReasonCodeDto> GetActiveReasons(this IEnumerable<OTReasonCodeDto> reasonList)
        {
            return reasonList.Where(x => x.IsActive).OrderBy(x => x.ReasonCode);
        }
    }
}
