

using FVN_REGISTER.Contract.Models;

namespace FVN_REGISTER.Contract.ViewModels
{
    public class LeaveDetailsViewModel
    {
        public VF03leaveDay? Header { get; set; }

        public List<LeaveDetailViewModel>? Days { get; set; }

        public List<F03leaveDaysAttachment>? Attachments { get; set; }

        public string? CurrentUserLevel { get; set; }

    }
}
