


    namespace FVN_REGISTER.Core.Constants
    {
        public static class ApproverRole
        {
            public const string SubLeader = "SubLeader";
            public const string Chief = "Chief";
            public const string Manager = "Manager";
            public const string GM = "GM";
            public const string Union = "Union";

            /// <summary>
            /// Ánh xạ level số (ApprovalLevel.*) sang tên vai trò hiển thị.
            /// </summary>
            public static string FromLevel(int approverLevel) => approverLevel switch
            {
                ApprovalLevel.SubLeader => SubLeader,
                ApprovalLevel.Chief => Chief,
                ApprovalLevel.Manager => Manager,
                ApprovalLevel.GM => GM,
                ApprovalLevel.Union => Union,
                _ => "Không xác định"
            };
        }
    }

