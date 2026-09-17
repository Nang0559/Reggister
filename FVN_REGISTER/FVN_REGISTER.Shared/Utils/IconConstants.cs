

namespace FVN_REGISTER.Shared.Utils
{
    public static class IconConstants
    {
        // 1. Module Icons
        public static class Module
        {
            public const string Leave = "Icons.Material.Filled.EventAvailable";    // Nghỉ phép
            public const string Overtime = "Icons.Material.Filled.AccessTime";     // Tăng ca
            public const string Trip = "Icons.Material.Filled.FlightTakeoff";     // Công tác
            public const string Default = "Icons.Material.Filled.Dashboard";
        }

        // 2. Status Icons (Trạng thái duyệt)
        public static class Status
        {
            public const string Draft = "Icons.Material.Filled.Edit";
            public const string Pending = "Icons.Material.Filled.Pending";
            public const string InProgress = "Icons.Material.Filled.Sync";
            public const string Approved = "Icons.Material.Filled.CheckCircle";
            public const string Rejected = "Icons.Material.Filled.Cancel";
            public const string Cancelled = "Icons.Material.Filled.HighlightOff";
            public const string Escalated = "Icons.Material.Filled.Warning";
        }

        // 3. Action Icons (Thao tác)
        public static class Action
        {
            public const string Create = "Icons.Material.Filled.Add";
            public const string Edit = "Icons.Material.Filled.EditNote";
            public const string Delete = "Icons.Material.Filled.Delete";
            public const string View = "Icons.Material.Filled.Visibility";
            public const string Save = "Icons.Material.Filled.Save";
            public const string Approve = "Icons.Material.Filled.ThumbUp";
            public const string Reject = "Icons.Material.Filled.ThumbDown";
            public const string Export = "Icons.Material.Filled.FileDownload";
            public const string Search = "Icons.Material.Filled.Search";
        }

        // 4. Notification Icons
        public static class Notification
        {
            public const string Reminder = "Icons.Material.Filled.NotificationsActive";
            public const string System = "Icons.Material.Filled.Info";
        }
        //5. Email
        public static class Email
        {
            public const string Pending = "Icons.Material.Filled.Schedule";
            public const string Processing = "Icons.Material.Filled.Sync";
            public const string Sent = "Icons.Material.Filled.MarkEmailRead";
            public const string Failed = "Icons.Material.Filled.ErrorOutline";
        }
    }
}
