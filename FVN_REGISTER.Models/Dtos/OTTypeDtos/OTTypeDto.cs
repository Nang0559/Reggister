namespace FVN_REGISTER.Contract.Dtos.OTTypeDtos
{
    public class OTTypeDto
    {
        public int Id { get; set; }
        public string OTTypeCode { get; set; } = string.Empty;
        public string OTTypeName { get; set; } = string.Empty;
        public string? OTTypeName2 { get; set; }
        public decimal RateMultiplier { get; set; }
        public string? HRMCode { get; set; }
        public bool IsActive { get; set; }
    }
}
