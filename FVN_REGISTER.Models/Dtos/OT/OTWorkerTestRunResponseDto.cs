using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.OT
{
    public sealed class OTWorkerTestRunResponseDto
    {
        public DateTime WorkDate { get; set; }

        public int SyncedFromHRM { get; set; }

        public OTReconciliationResultDto Reconciliation { get; set; } = default!;
    }
}
