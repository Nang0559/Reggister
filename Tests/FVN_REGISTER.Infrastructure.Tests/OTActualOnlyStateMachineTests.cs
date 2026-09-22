using FVN_REGISTER.Contract.Dtos.Execution;
using Xunit;

namespace FVN_REGISTER.Infrastructure.Tests;

public sealed class OTActualOnlyStateMachineTests
{
    [Fact]
    public void AutoResolve_WhenApprovedOtAppears_IsNotCancellation()
    {
        var request = CreateResolvedRequest("OT_REQUEST_EXISTS", isCancellation: false);

        Assert.Equal("Resolved", request.ReconciliationStatus);
        Assert.False(request.IsCancellation);
    }

    [Fact]
    public void AutoResolve_WhenActualOtDisappears_IsNotCancellation()
    {
        var request = CreateResolvedRequest("ACTUAL_OT_NO_LONGER_PRESENT", isCancellation: false);

        Assert.Equal("Resolved", request.ReconciliationStatus);
        Assert.False(request.IsCancellation);
    }

    [Fact]
    public void LegacyResolvedRequest_RemainsCancellationCompatible()
    {
        var request = CreateResolvedRequest("LEGACY_CANCEL");

        Assert.True(request.IsCancellation);
    }

    private static ExecutionReconciliationUpsertRequest CreateResolvedRequest(
        string reason,
        bool isCancellation = true)
        => new(
            "OT",
            "OT_ACTUAL_ONLY",
            "E0001:2026-09-22",
            "E0001",
            1,
            new DateOnly(2026, 9, 22),
            "SHIFT/OT",
            "OT",
            "Resolved",
            false,
            false,
            $"{{\"AutoResolvedReason\":\"{reason}\"}}",
            isCancellation);
}
