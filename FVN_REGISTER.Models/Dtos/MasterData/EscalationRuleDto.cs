
using FVN_REGISTER.Core.Enums;


namespace FVN_REGISTER.Contract.Dtos.MasterData
{
    public class EscalationRuleDto
    {
        public RequestModule RequestType { get; set; }
        public int Level { get; set; }
        public string? DeptCode { get; set; }
        public decimal WarningHours { get; set; }
        public decimal EscalateHours { get; set; }
        public int DeadlineHour { get; set; }
    }
}
