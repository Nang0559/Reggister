using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Utils
{
    public static class OTCreatorLevel
    {
        public const string Worker = "Worker";      // Công nhân
        public const string Office = "Office";      // NV Văn phòng (bỏ qua Lv3)
        public const string SubLeader = "SubLeader";
        public const string Leader = "Leader";
        public const string UnionRep = "UnionRep";    // BCH CĐ
        public const string AstChief = "AstChief";    // Từ cấp này → bắt buộc GM
        public const string Chief = "Chief";
        public const string AMG = "AMG";
        public const string MG = "MG";
        public const string GM = "GM";

        // Nếu người tạo từ Ast.Chief trở lên → bắt buộc GM ký
        public static bool RequiresGM(string level) =>
            level is AstChief or Chief or AMG or MG or GM;

        // Nếu NV văn phòng → bỏ qua bước 3 (Sub.Leader)
        public static bool SkipLv3(string level) =>
            level == Office;
    }
}
