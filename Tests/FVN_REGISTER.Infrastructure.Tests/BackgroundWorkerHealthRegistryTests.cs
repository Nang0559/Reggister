using FVN_REGISTER.Infrastructure.Services.Jobs;
using Xunit;

namespace FVN_REGISTER.Infrastructure.Tests;

public sealed class BackgroundWorkerHealthRegistryTests
{
    [Fact]
    public void Registry_tracks_success_and_failure_without_losing_last_success()
    {
        var registry = new BackgroundWorkerHealthRegistry();
        registry.Started("HRM");
        registry.Success("HRM");
        registry.Failure("HRM", new InvalidOperationException("temporary failure"));

        var state = Assert.Single(registry.Snapshot());
        Assert.Equal("HRM", state.Name);
        Assert.NotNull(state.LastSuccessAt);
        Assert.NotNull(state.LastFailureAt);
        Assert.Equal("temporary failure", state.LastError);
    }
}
