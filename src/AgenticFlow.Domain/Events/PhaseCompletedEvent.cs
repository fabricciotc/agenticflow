using System.Text.Json.Serialization;

namespace AgenticFlow.Domain.Events;

/// <summary>
/// Raised when an orchestrator phase completes.
/// </summary>
public class PhaseCompletedEvent : DomainEventBase
{
    [JsonPropertyName("ticketId")]
    public string TicketId { get; set; } = string.Empty;

    [JsonPropertyName("phase")]
    public string Phase { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
