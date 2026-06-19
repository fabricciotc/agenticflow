using System.Text.Json.Serialization;

namespace AgenticFlow.Domain.Events;

/// <summary>
/// Raised when a new domain message is published.
/// </summary>
public class MessagePublishedEvent : DomainEventBase
{
    [JsonPropertyName("messageId")]
    public Guid MessageId { get; set; }

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}
