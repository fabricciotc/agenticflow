namespace AgenticFlow.Domain.Entities;

public class Ticket : Entity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public Dictionary<string, object> Metadata { get; set; } = new();
}
