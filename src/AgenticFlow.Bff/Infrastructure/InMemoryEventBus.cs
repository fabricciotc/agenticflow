using AgenticFlow.Application.Abstractions;
using AgenticFlow.Domain.Events;

namespace AgenticFlow.Bff.Infrastructure;

public class InMemoryEventBus : IEventBus
{
    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
