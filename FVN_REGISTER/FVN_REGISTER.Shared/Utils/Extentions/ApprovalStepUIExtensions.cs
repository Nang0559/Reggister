using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Shared.Constants;


namespace FVN_REGISTER.Shared.Utils.Extentions
{
    public static class ApprovalStepUIExtensions
    {
        public static string GetStatusColor(this ApprovalStepDto step) => step.IsApproved switch
        {
            true => UIConstants.ColorSuccess,
            false => UIConstants.ColorError,
            null => UIConstants.ColorWarning
        };
       
    }
}
