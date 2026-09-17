


using FVN_REGISTER.Contract.Dtos.OT;

namespace FVN_REGISTER.Application.Interfaces.Jobs
{
    public interface IOTWorkerStatus
    {
        OTWorkerStatusDto GetStatus();
    }
}
