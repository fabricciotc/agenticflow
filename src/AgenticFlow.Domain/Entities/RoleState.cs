using System.Text.Json.Serialization;

namespace AgenticFlow.Domain.Entities;

/// <summary>
/// Runtime state of a role/agent displayed in the dashboard.
/// Maps to the legacy Python agent entries kept in the run-state <c>agents</c> list.
/// </summary>
public class RoleState
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("parentId")]
    public string? ParentId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "queued";

    [JsonPropertyName("progress")]
    public int Progress { get; set; }

    [JsonPropertyName("logs")]
    public List<RoleLogEntry> Logs { get; set; } = new();

    [JsonPropertyName("outputs")]
    public List<string> Outputs { get; set; } = new();

    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// A single log entry for a role/agent.
/// </summary>
public class RoleLogEntry
{
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("level")]
    public string Level { get; set; } = "info";

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
