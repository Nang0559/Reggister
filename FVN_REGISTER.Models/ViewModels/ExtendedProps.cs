

namespace FVN_REGISTER.Contract.ViewModels
{
    public class ExtendedProps
    {
        public DateTime? registerdate { get; set; }

        public decimal daylev { get; set; } = 0;     // 🔥 FIX
        public decimal totalDay { get; set; } = 0;   // 🔥 FIX

        public string description { get; set; } = string.Empty;
        public string levelresion { get; set; } = string.Empty;
        public string APstatus { get; set; } = string.Empty;

        public object details { get; set; }

        public int TinhPhep { get; set; } = 1;

        public string level1ApprovedBy { get; set; } = string.Empty;
        public string Level1ApproveEmail { get; set; } = string.Empty;
        public DateTime? level1ApprovedDate { get; set; }   // 🔥 FIX
        public string level1Comment { get; set; } = string.Empty;

        public string level2ApprovedBy { get; set; } = string.Empty;
        public string Level2ApproveEmail { get; set; } = string.Empty;
        public DateTime? level2ApprovedDate { get; set; }   // 🔥 FIX
        public string level2Comment { get; set; } = string.Empty;

        public string level3ApprovedBy { get; set; } = string.Empty;
        public string Level3ApproveEmail { get; set; } = string.Empty;
        public DateTime? level3ApprovedDate { get; set; }   // 🔥 FIX
        public string level3Comment { get; set; } = string.Empty;
    }
}
