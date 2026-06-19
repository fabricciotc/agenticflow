using AgenticFlow.Application.Abstractions;
using AgenticFlow.Persistence.JsonStores;

namespace AgenticFlow.Persistence.Repositories;

public class BoardStore : IBoardStore
{
    private readonly JsonFileStore<BoardState> _store;

    public BoardStore(JsonFileStore<BoardState> store)
    {
        _store = store;
    }

    public BoardState Load() => _store.Load();
    public void Save(BoardState state) => _store.Save(state);
}
