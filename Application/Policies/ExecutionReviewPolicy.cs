namespace FVN_REGISTER.Application.Policies;

public static class ExecutionReviewPolicy
{
    public static string NormalizeEvidenceReviewStatus(string value)
        => value?.Trim().ToUpperInvariant() switch
        {
            "APPROVED" => "Approved",
            "REJECTED" => "Rejected",
            "NEEDMOREEVIDENCE" => "NeedMoreEvidence",
            "NEED_MORE_EVIDENCE" => "NeedMoreEvidence",
            _ => throw new ArgumentException(
                "ReviewStatus chỉ được là Approved, Rejected hoặc NeedMoreEvidence.")
        };

    public static string ConfirmationStatusAfterEvidenceReview(string reviewStatus)
        => NormalizeEvidenceReviewStatus(reviewStatus) == "Approved"
            ? "Pending"
            : "NeedMoreEvidence";

    public static string? PreserveEmployeeDecision(string? employeeDecision)
        => employeeDecision;
}
