using System.Collections.Concurrent;

namespace FVN_REGISTER.Infrastructure.Services.Jobs;

public sealed record BackgroundWorkerHealthSnapshot(
    string Name,
    DateTimeOffset StartedAt,
    DateTimeOffset? LastSuccessAt,
    DateTimeOffset? LastFailureAt,
    string? LastError);

public sealed class BackgroundWorkerHealthRegistry
{
    private sealed class State
    {
        public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? LastSuccessAt { get; set; }
        public DateTimeOffset? LastFailureAt { get; set; }
        public string? LastError { get; set; }
    }

    private readonly ConcurrentDictionary<string, State> _states = new(StringComparer.OrdinalIgnoreCase);

    public void Started(string name)
        => _states.TryAdd(name, new State());

    public void Success(string name)
    {
        var state = _states.GetOrAdd(name, _ => new State());
        state.LastSuccessAt = DateTimeOffset.UtcNow;
        state.LastError = null;
    }

    public void Failure(string name, Exception ex)
    {
        var state = _states.GetOrAdd(name, _ => new State());
        state.LastFailureAt = DateTimeOffset.UtcNow;
        state.LastError = ex.Message;
    }

    public IReadOnlyList<BackgroundWorkerHealthSnapshot> Snapshot()
        => _states.Select(x => new BackgroundWorkerHealthSnapshot(
                x.Key,
                x.Value.StartedAt,
                x.Value.LastSuccessAt,
                x.Value.LastFailureAt,
                x.Value.LastError))
            .OrderBy(x => x.Name)
            .ToList();
}
