

namespace FVN_REGISTER.Contract.Dtos.OtReasons
{
    public class OTReasonCodeDto
    {
        public int Id { get; set; }
        public string ReasonCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
