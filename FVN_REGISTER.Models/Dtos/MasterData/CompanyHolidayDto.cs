

namespace FVN_REGISTER.Contract.Dtos.MasterData
{
    public class CompanyHolidayDto
    {
        public int Id { get; set; }
        public DateTime HolidayDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Year { get; set; }
        public bool IsPaidLeave { get; set; } = true;
    }
}
