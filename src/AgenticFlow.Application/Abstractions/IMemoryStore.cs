using AgenticFlow.Domain.Entities;

namespace AgenticFlow.Application.Abstractions;

public interface IMemoryStore
{
    void Add(Message message);
    IReadOnlyList<Message> GetAll();
    IReadOnlyList<Message> GetByCause(string cause);
    IReadOnlyList<Message> GetByRole(string role);
    IReadOnlyList<Message> GetByType(string type);
    IReadOnlyList<Message> GetByRecipient(string recipient);
}
