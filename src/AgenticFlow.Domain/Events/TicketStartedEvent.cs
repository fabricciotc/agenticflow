using System.Text.Json.Serialization;

namespace AgenticFlow.Domain.Events;

/// <summary>
/// Raised when a ticket run is started or resumed.
/// </summary>
public class TicketStartedEvent : DomainEventBase
{
    [JsonPropertyName("ticketId")]
    public string TicketId { get; set; } = string.Empty;

    [JsonPropertyName("resumed")]
    public bool Resumed { get; set; }
}
