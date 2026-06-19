using AgenticFlow.Application.Abstractions;
using AgenticFlow.Persistence.JsonStores;

namespace AgenticFlow.Persistence.Repositories;

public class ConfigStore : IConfigStore
{
    private readonly JsonFileStore<AppConfig> _store;

    public ConfigStore(JsonFileStore<AppConfig> store)
    {
        _store = store;
    }

    public AppConfig Load() => _store.Load();
    public void Save(AppConfig config) => _store.Save(config);
}
