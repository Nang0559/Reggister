

namespace FVN_REGISTER.Contract.ViewModels
{
    public class WorkYearStatsViewModel
    {
        public int TotalWorkYears { get; set; }

        public int ActiveWorkYears { get; set; }

        public int CurrentYear { get; set; }

        public int? ActiveYear { get; set; }

        public List<int> AllYears { get; set; } = new List<int>();
    }
}
