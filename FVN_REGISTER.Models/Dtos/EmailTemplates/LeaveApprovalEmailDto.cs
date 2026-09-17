namespace FVN_REGISTER.Contract.Dtos.EmailTemplates
{
    public sealed record LeaveApprovalEmailDto(
        string EmployeeName,
        string RecipientRole,
        DateTime StartDate,
        DateTime EndDate,
        decimal TotalDay,
        string Reason,
        int LeaveId);
}
