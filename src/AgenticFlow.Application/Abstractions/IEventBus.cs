using AgenticFlow.Domain.Events;

namespace AgenticFlow.Application.Abstractions;

public interface IEventBus
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
