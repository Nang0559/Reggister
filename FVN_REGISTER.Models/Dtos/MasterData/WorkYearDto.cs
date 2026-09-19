namespace FVN_REGISTER.Contract.Dtos.MasterData
{
    public class WorkYearDto
    {
        public int Id { get; set; }

        public int Year { get; set; } // Ví dụ: 2026

        public string YearName { get; set; } = string.Empty; // Ví dụ: "Năm 2026"

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }
        public string? Remark { get; set; }
    }
}
