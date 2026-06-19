using AgenticFlow.Domain.Entities;

namespace AgenticFlow.Application.Abstractions;

public interface IBoardStore
{
    BoardState Load();
    void Save(BoardState state);
}

public class BoardState
{
    public List<Ticket> Tickets { get; set; } = new();
}
