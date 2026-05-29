

namespace FVN_REGISTER.Contract.ViewModels
{
    public class HolidayViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string Inforregister { get; set; }
        public string Status { get; set; }
        public ExtendedProps ExtendedProps { get; set; }
        = new ExtendedProps();
        public List<string> ClassNames { get; set; } = new List<string>();
    }
}
