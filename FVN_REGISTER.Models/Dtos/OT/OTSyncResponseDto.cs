using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Contract.Dtos.OT
{
    public sealed class OTSyncResponseDto
    {
        public DateTime WorkDate { get; set; }

        public int SyncedCount { get; set; }
    }
}
