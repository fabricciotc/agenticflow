using AgenticFlow.Domain.Entities;

namespace AgenticFlow.Application.Abstractions;

public interface IContext
{
    Ticket Ticket { get; }
    IMemoryStore Memory { get; }
    CancellationToken CancellationToken { get; }
}
