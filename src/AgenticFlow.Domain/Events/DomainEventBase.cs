using System.Text.Json.Serialization;

namespace AgenticFlow.Domain.Events;

/// <summary>
/// Base class for all domain events.
/// </summary>
public abstract class DomainEventBase : IDomainEvent
{
    [JsonPropertyName("eventId")]
    public Guid EventId { get; } = Guid.NewGuid();

    [JsonPropertyName("occurredOn")]
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
