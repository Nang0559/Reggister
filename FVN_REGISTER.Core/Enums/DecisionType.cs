namespace FVN_REGISTER.Core.Enums
{
    /// <summary>
    /// Kết quả của một approval step.
    /// Escalated là trạng thái hệ thống khi step hết thời gian chờ và được chuyển quyền xử lý
    /// lên cấp tiếp theo; đây không phải là một quyết định Reject của approver.
    /// </summary>
    public enum DecisionType
    {
        Pending,
        Approved,
        Rejected,
        Returned,
        Escalated
    }
}
