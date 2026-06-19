using AgenticFlow.Domain.Entities;

namespace AgenticFlow.Application.Abstractions;

public interface ITicketService
{
    Task<IReadOnlyList<Ticket>> GetAllAsync();
    Task<Ticket?> GetByIdAsync(Guid id);
    Task PlayAsync(Guid id);
}
