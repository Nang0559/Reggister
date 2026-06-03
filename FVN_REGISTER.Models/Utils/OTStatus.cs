using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Utils
{
    // OTStatus.cs
    public static class OTStatus
    {
        public const string Pending = "Pending";      // Chờ Union duyệt
        public const string UnionApproved = "UnionApproved"; // Union OK, chờ Chief
        public const string ChiefApproved = "ChiefApproved"; // Chief OK, chờ MG
        public const string MGApproved = "MGApproved";    // MG OK (→ chờ GM nếu cần)
        public const string Approved = "Approved";      // Hoàn tất duyệt
        public const string Rejected = "Rejected";
        public const string Cancelled = "Cancelled";

        // Mapping approver level → status tiếp theo
        public static string NextStatus(string currentLevel, bool requiresGM) => currentLevel switch
        {
            "Union" => ChiefApproved,  // sai, phải là UnionApproved → Chief pending
            "Chief" => ChiefApproved,
            "MG" => requiresGM ? MGApproved : Approved,
            "GM" => Approved,
            _ => Pending
        };
    }
}
