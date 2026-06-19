using AgenticFlow.Application.Abstractions;
using AgenticFlow.Persistence.JsonStores;

namespace AgenticFlow.Persistence.Repositories;

public class RunStateStore : IRunStateStore
{
    private readonly JsonFileStore<RunState> _store;

    public RunStateStore(JsonFileStore<RunState> store)
    {
        _store = store;
    }

    public RunState Load() => _store.Load();
    public void Save(RunState state) => _store.Save(state);
}
