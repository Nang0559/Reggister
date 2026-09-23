using FVN_REGISTER.Application.Policies;
using Xunit;

namespace FVN_REGISTER.Infrastructure.Tests;

public sealed class ExecutionReviewPolicyTests
{
    [Theory]
    [InlineData("Approved", "Approved")]
    [InlineData("Rejected", "Rejected")]
    [InlineData("NeedMoreEvidence", "NeedMoreEvidence")]
    [InlineData("NEED_MORE_EVIDENCE", "NeedMoreEvidence")]
    public void Evidence_review_status_is_normalized(string input, string expected)
        => Assert.Equal(expected, ExecutionReviewPolicy.NormalizeEvidenceReviewStatus(input));

    [Fact]
    public void Rejected_or_need_more_evidence_keeps_confirmation_open()
    {
        Assert.Equal(
            "NeedMoreEvidence",
            ExecutionReviewPolicy.ConfirmationStatusAfterEvidenceReview("Rejected"));

        Assert.Equal(
            "NeedMoreEvidence",
            ExecutionReviewPolicy.ConfirmationStatusAfterEvidenceReview("NeedMoreEvidence"));
    }

    [Fact]
    public void HR_resolution_does_not_overwrite_employee_decision()
    {
        Assert.Equal(
            "REJECTED",
            ExecutionReviewPolicy.PreserveEmployeeDecision("REJECTED"));

        Assert.Null(ExecutionReviewPolicy.PreserveEmployeeDecision(null));
    }
}
