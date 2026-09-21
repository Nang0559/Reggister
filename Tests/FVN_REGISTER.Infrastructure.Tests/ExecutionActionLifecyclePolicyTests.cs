using FVN_REGISTER.Core.Entities.WorkCalendar;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Infrastructure.Tests;

public sealed class ExecutionActionLifecyclePolicyTests
{
    [Theory]
    [InlineData("Resolved", ActionItemStatus.Completed)]
    [InlineData("Mismatch", ActionItemStatus.InProgress)]
    [InlineData("AwaitingConfirmation", ActionItemStatus.InProgress)]
    [InlineData("Matched", ActionItemStatus.Cancelled)]
    [InlineData("None", ActionItemStatus.Expired)]
    public void ResolveDueStatus_ReturnsExpectedTransition(
        string reconciliationStatus,
        ActionItemStatus expected)
    {
        Assert.Equal(
            expected,
            ExecutionActionLifecyclePolicy.ResolveDueStatus(reconciliationStatus));
    }

    [Fact]
    public void ResolveDueStatus_UnknownStatus_ExpiresAction()
    {
        Assert.Equal(
            ActionItemStatus.Expired,
            ExecutionActionLifecyclePolicy.ResolveDueStatus("Unexpected"));
    }
}
