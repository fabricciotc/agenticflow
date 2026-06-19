namespace AgenticFlow.Application.Abstractions;

public interface ISnapshotStore
{
    void Save(string ticketId, Snapshot snapshot);
    Snapshot? Load(string ticketId);
}

public class Snapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TicketId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Dictionary<string, object> State { get; set; } = new();
}
