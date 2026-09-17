using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Shared.Constants;


namespace FVN_REGISTER.Shared.Utils.Extentions
{
    public static class WorkerRunStateUIExtensions
    {
        public static string ToColor(this WorkerRunState state) => state switch
        {
            WorkerRunState.Waiting => UIConstants.ColorInfo,
            WorkerRunState.Running => UIConstants.ColorWarning,
            WorkerRunState.Error => UIConstants.ColorError,
            WorkerRunState.Stopped => UIConstants.ColorDefault,
            _ => UIConstants.ColorDefault
        };
    }
}
