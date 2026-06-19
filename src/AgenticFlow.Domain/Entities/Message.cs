namespace AgenticFlow.Domain.Entities;

public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Cause { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
