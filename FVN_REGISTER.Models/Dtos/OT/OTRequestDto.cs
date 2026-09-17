
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Interfaces;
using FVN_REGISTER.Contract.Requests;
using FVN_REGISTER.Core.Interfaces;


    namespace FVN_REGISTER.Contract.Dtos.OT
    {
    public class OTRequestDto : BaseRequestDto<OTEmployeeDto>, IPendingRequestRow
    {
        public int WorkYear { get; set; } = DateTime.Now.Year;
        public DateTime OTDate { get; set; } = DateTime.Now;
        public string OTTypeCode { get; set; } = string.Empty;
        public string OtPurpose { get; set; } = string.Empty;
        public decimal TotalOtHours => Details?.Sum(x => x.OTHours) ?? 0m;
        public bool HasWorker { get; set; }
        public int RequestId => Id;
    }
}

